using System.Buffers;
using System.Diagnostics.Tracing;
using System.Text;
using NetTrace.FastSerializer;

namespace NetTrace.Test;

[TestFixture]
public class EventMetadataReaderTest
{
    [TestCase(false)]
    [TestCase(true)]
    public void ProviderAndFieldNames_AreInternedAcrossMetadataRows(bool isV6)
    {
        byte[] bytes = isV6
            ? V6(w => { w.Write((ushort)1); V6Field(w, "Shared雪", f => f.Write((byte)9)); })
            : V5(w => { w.Write(1); w.Write(9); Utf16(w, "Shared雪"); });
        EventMetadataReader decoder = new();
        var first = Decode(decoder, bytes, isV6);
        var second = Decode(decoder, bytes, isV6);
        Assert.Multiple(() =>
        {
            Assert.That(first.ProviderName, Is.SameAs(second.ProviderName));
            Assert.That(first.FieldDefinitions[0].Name, Is.SameAs(second.FieldDefinitions[0].Name));
            Assert.That(first.FieldDefinitions[0].Name, Is.SameAs(first.ProviderName));
        });
    }

    [Test]
    public void V5ParameterPayload_DecodesBoundedRecursiveFieldsAndSkipsExtensions()
    {
        var parameterPayload = Bytes(w =>
        {
            w.Write(2);
            V2Field(w, f =>
            {
                f.Write(1); // object
                f.Write(1);
                V2Field(f, nested => { nested.Write(9); Utf16(nested, "Value"); nested.Write((byte)0xff); });
                Utf16(f, "Object");
                f.Write((byte)0xee);
            });
            V2Field(w, f => { f.Write(19); f.Write(8); Utf16(f, "Array"); });
            w.Write((byte)0xdd); // tag extension
        });
        var bytes = V5(w =>
        {
            w.Write(0); // empty V1 fields replaced by ParameterPayload
            Tag(w, 2, parameterPayload);
            Tag(w, 0xff, [1, 2, 3]); // unknown tags remain forward compatible
            Tag(w, 1, [0xee, 0xcc]); // opcode and a tag extension
        });
        var metadata = Decode(new EventMetadataReader(), bytes, false);
        Assert.Multiple(() =>
        {
            Assert.That(metadata.FieldDefinitions, Has.Count.EqualTo(2));
            Assert.That(metadata.FieldDefinitions[0].TypeCode, Is.EqualTo(NetTraceTypeCode.Object));
            Assert.That(metadata.FieldDefinitions[0].SubFieldDefinitions![0].Name, Is.EqualTo("Value"));
            Assert.That(metadata.FieldDefinitions[1].Name, Is.EqualTo("Array"));
            Assert.That(metadata.FieldDefinitions[1].ArrayTypeCode, Is.EqualTo(NetTraceTypeCode.UInt16));
            Assert.That(metadata.Opcode, Is.EqualTo((EventOpcode)0xee));
        });
    }

    [TestCase("name")]
    [TestCase("type")]
    [TestCase("element")]
    [TestCase("count")]
    [TestCase("object")]
    public void V6Field_CannotBorrowBytesFromSiblingField(string corruption)
    {
        long siblingOffset = 0;
        var bytes = V6(w =>
        {
            w.Write((ushort)2);
            var brokenField = Bytes(f =>
            {
                if (corruption == "name")
                {
                    f.Write7BitEncodedInt(2);
                    f.Write((byte)'X'); // missing second UTF8 name byte
                    return;
                }
                Text(f, "Broken");
                switch (corruption)
                {
                    case "type": break; // missing type byte
                    case "element": f.Write((byte)19); break; // missing array element type
                    case "count": f.Write((byte)22); f.Write((byte)6); break; // missing fixed count
                    case "object": f.Write((byte)1); f.Write((byte)1); break; // partial child field count
                }
            });
            w.Write(checked((ushort)brokenField.Length));
            w.Write(brokenField);
            siblingOffset = w.BaseStream.Position;
            V6Field(w, "Sibling", f => f.Write((byte)9));
        });
        FastSerializerSequenceReader reader = new(new ReadOnlySequence<byte>(bytes), 0);
        try
        {
            new EventMetadataReader().ReadEventMetadataV6(ref reader);
            Assert.Fail("Incomplete field must fail within its declared boundary");
        }
        catch (EndOfStreamException)
        {
            Assert.That(reader.Consumed, Is.EqualTo(siblingOffset), "sibling field must remain unread");
        }
    }

    [Test]
    public void V2Field_CannotReadNameTerminatorFromSiblingField()
    {
        var bytes = V5(w =>
        {
            w.Write(0);
            Tag(w, 2, Bytes(p =>
            {
                p.Write(2);
                V2Field(p, f => { f.Write(9); f.Write((ushort)'X'); }); // no UTF16 terminator
                V2Field(p, f => { f.Write(9); Utf16(f, "Sibling"); });
            }));
        });
        Assert.Throws<EndOfStreamException>(() => Decode(new EventMetadataReader(), bytes, false));
    }

    [TestCase(1)]
    [TestCase(2)]
    public void V5Tag_CannotBorrowBytesFromFollowingTag(int tag)
    {
        var bytes = V5(w =>
        {
            w.Write(0);
            Tag(w, (byte)tag, tag == 1 ? [] : [1, 0, 0]); // opcode missing; partial field count
            Tag(w, 1, [2]);
        });
        Assert.Throws<EndOfStreamException>(() => Decode(new EventMetadataReader(), bytes, false));
    }

    [Test]
    public void EnumCasts_PreserveWireWidthAndUnknownValues()
    {
        var v5 = V5(w => { w.Write(1); w.Write(0x12345678); Utf16(w, "Unknown"); }, level: 0x12345678);
        var v6 = V6(w =>
        {
            w.Write((ushort)1);
            V6Field(w, "Unknown", f => f.Write((byte)0xfe));
            var optional = Bytes(o =>
            {
                o.Write((byte)1); o.Write((byte)0xee);
                o.Write((byte)8); o.Write((byte)0xff);
                o.Write((byte)3); o.Write(long.MinValue);
            });
            w.Write(checked((ushort)optional.Length));
            w.Write(optional);
        });
        var legacy = Decode(new EventMetadataReader(), v5, false);
        var current = Decode(new EventMetadataReader(), v6, true);
        Assert.Multiple(() =>
        {
            Assert.That(legacy.Level, Is.EqualTo((EventLevel)0x12345678));
            Assert.That(legacy.FieldDefinitions[0].TypeCode, Is.EqualTo((NetTraceTypeCode)0x12345678));
            Assert.That(current.Level, Is.EqualTo((EventLevel)0xff));
            Assert.That(current.Opcode, Is.EqualTo((EventOpcode)0xee));
            Assert.That(current.Keywords, Is.EqualTo((EventKeywords)long.MinValue));
            Assert.That(current.FieldDefinitions[0].TypeCode, Is.EqualTo((NetTraceTypeCode)0xfe));
        });
    }

    private static EventMetadata Decode(EventMetadataReader decoder, byte[] bytes, bool isV6)
    {
        FastSerializerSequenceReader reader = new(new ReadOnlySequence<byte>(bytes), 0);
        var metadata = isV6 ? decoder.ReadEventMetadataV6(ref reader) : decoder.ReadEventMetadata(ref reader);
        Assert.That(reader.End, Is.True);
        return metadata;
    }

    private static byte[] V5(Action<BinaryWriter> fieldsAndTags, int level = 4)
    {
        return Bytes(w =>
        {
            w.Write(1); Utf16(w, "Shared雪"); w.Write(123); Utf16(w, "Synthetic");
            w.Write(0L); w.Write(0); w.Write(level);
            fieldsAndTags(w);
        });
    }

    private static byte[] V6(Action<BinaryWriter> fieldsAndOptional)
    {
        return Bytes(w =>
        {
            w.Write7BitEncodedInt(1); Text(w, "Shared雪"); w.Write7BitEncodedInt(123); Text(w, "Synthetic");
            fieldsAndOptional(w);
        });
    }

    private static void V2Field(BinaryWriter writer, Action<BinaryWriter> content)
    {
        var bytes = Bytes(content);
        writer.Write(bytes.Length + sizeof(int));
        writer.Write(bytes);
    }

    private static void V6Field(BinaryWriter writer, string name, Action<BinaryWriter> type)
    {
        var bytes = Bytes(w => { Text(w, name); type(w); });
        writer.Write(checked((ushort)bytes.Length));
        writer.Write(bytes);
    }

    private static void Tag(BinaryWriter writer, byte tag, byte[] payload)
    {
        writer.Write(payload.Length);
        writer.Write(tag);
        writer.Write(payload);
    }

    private static void Utf16(BinaryWriter writer, string value)
    {
        writer.Write(Encoding.Unicode.GetBytes(value));
        writer.Write((ushort)0);
    }

    private static void Text(BinaryWriter writer, string value)
    {
        var bytes = Encoding.UTF8.GetBytes(value);
        writer.Write7BitEncodedInt(bytes.Length);
        writer.Write(bytes);
    }

    private static byte[] Bytes(Action<BinaryWriter> write)
    {
        using var stream = new MemoryStream();
        using (var writer = new BinaryWriter(stream, Encoding.UTF8, leaveOpen: true)) write(writer);
        return stream.ToArray();
    }
}
