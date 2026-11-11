using static NetTrace.Test.V6TraceFixture;

namespace NetTrace.Test;

// Schema selection and CLR-specific layouts, independent of V6 block/table mechanics.
[TestFixture]
public class RuntimeEventSchemaTest
{
    private const long OsThread = 0x100000123;
    private const long CaptureOsThread = 0x200000456;
    private const long OsProcess = 0x300000789;

    [Test]
    public async Task RuntimeMetadataWithoutFields_UsesLabeledVersionForSchemaFallback()
    {
        var fixture = new V6TraceFixture();
        fixture.Thread(1, OsThread, OsProcess, "worker");
        fixture.Thread(2, CaptureOsThread, 99, "capture");
        fixture.Metadata(1, "", provider: "Microsoft-Windows-DotNETRuntime", eventId: 1, version: 0);
        fixture.Labels(1, [Label(10, b => b.U8(2))]);
        fixture.Events(true, new Row(LabelId: 1, Payload: Bytes(b =>
        {
            b.U32(7); b.U32(2); b.U32(3); b.U32(1); b.U16(4); b.U64(ulong.MaxValue);
        })));
        var evt = (await ReadTraceAsync(fixture.Finish())).Events.Single();
        Assert.Multiple(() =>
        {
            Assert.That(evt.Metadata.Version, Is.EqualTo(2));
            Assert.That(evt.Payload["Count"], Is.EqualTo(7u));
            Assert.That(evt.Payload["Depth"], Is.EqualTo(2u));
            Assert.That(evt.Payload["ClrInstanceID"], Is.EqualTo((ushort)4));
            Assert.That(evt.Payload["ClientSequenceNumber"], Is.EqualTo(ulong.MaxValue));
        });
    }

    [TestCase(false)]
    [TestCase(true)]
    public async Task RuntimeMissingSchemaVersion_SelectsCompletePayloadLayoutAndPreservesDeclaredVersion(bool compressed)
    {
        var fixture = new V6TraceFixture();
        fixture.Thread(1, OsThread, OsProcess, "worker");
        fixture.Thread(2, CaptureOsThread, 99, "capture");
        fixture.Metadata(1, "Unknown(1)", provider: "Microsoft-Windows-DotNETRuntime", eventId: 1, version: 0);
        fixture.Events(compressed, new Row(Payload: Bytes(b =>
        {
            b.U32(7); b.U32(2); b.U32(3); b.U32(1); b.U16(4); b.U64(99);
        })));
        var trace = await ReadTraceAsync(fixture.Finish());
        var evt = trace.Events.Single();
        Assert.Multiple(() =>
        {
            Assert.That(evt.Metadata.Version, Is.Zero);
            Assert.That(evt.Metadata.EventName, Is.EqualTo("GCStart_V2"));
            Assert.That(evt.Metadata.FieldDefinitions.Select(f => f.Name), Does.Contain("Depth"));
            Assert.That(evt.Payload["Depth"], Is.EqualTo(2u));
            Assert.That(evt.Payload["ClientSequenceNumber"], Is.EqualTo(99UL));
            Assert.That(trace.EventMetadata, Does.Contain(evt.Metadata));
        });
    }

    [Test]
    public async Task RuntimeBulkType_DecodesNamedCountsAndNestedTypeParameters()
    {
        var fixture = new V6TraceFixture();
        fixture.Thread(1, OsThread, OsProcess, null);
        fixture.Thread(2, CaptureOsThread, 99, null);
        fixture.Metadata(1, "", provider: "Microsoft-Windows-DotNETRuntime", eventId: 15, version: 0);
        fixture.Events(true, new Row(Payload: Bytes(b =>
        {
            b.U32(2); b.U16(4);
            b.U64(10); b.U64(20); b.U32(30); b.U32(40); b.U8(5); b.Utf16("Generic雪");
            b.U32(2); b.U64(100); b.U64(200);
            b.U64(11); b.U64(21); b.U32(31); b.U32(41); b.U8(6); b.Utf16("Simple"); b.U32(0);
        })));
        var evt = (await ReadTraceAsync(fixture.Finish())).Events.Single();
        var types = (object[])evt.Payload["Values"];
        var first = (IReadOnlyDictionary<string, object>)types[0];
        var second = (IReadOnlyDictionary<string, object>)types[1];
        Assert.Multiple(() =>
        {
            Assert.That(evt.Payload["Count"], Is.EqualTo(2u));
            Assert.That(types, Has.Length.EqualTo(2));
            Assert.That(first["Name"], Is.EqualTo("Generic雪"));
            Assert.That(first["TypeParameters"], Is.EqualTo(new object[] { 100UL, 200UL }));
            Assert.That(second["TypeParameters"], Is.Empty);
        });
    }

    [Test]
    public async Task RuntimeAllocationSample_DecodesPayloadWithoutSchema()
    {
        var fixture = new V6TraceFixture();
        fixture.Thread(1, OsThread, OsProcess, null);
        fixture.Thread(2, CaptureOsThread, 99, null);
        fixture.Metadata(1, "", provider: "Microsoft-Windows-DotNETRuntime", eventId: 303, version: 0);
        fixture.Events(false, new Row(Payload: Bytes(b =>
        {
            b.U32(1); b.U16(2); b.U64(0x123456789); b.Utf16("System.Byte[]");
            b.U64(0x987654321); b.U64(100_000); b.U64(1234);
        })));
        var payload = (await ReadTraceAsync(fixture.Finish())).Events.Single().Payload;
        Assert.Multiple(() =>
        {
            Assert.That(payload["AllocationKind"], Is.EqualTo(1u));
            Assert.That(payload["TypeName"], Is.EqualTo("System.Byte[]"));
            Assert.That(payload["ObjectSize"], Is.EqualTo(100_000UL));
            Assert.That(payload["SampledByteOffset"], Is.EqualTo(1234UL));
        });
    }

    [TestCase(false)]
    [TestCase(true)]
    public async Task SchemaLessUnrelatedProvider_PreservesNonemptyRawPayloadAndEventBoundaries(bool compressed)
    {
        var fixture = new V6TraceFixture();
        fixture.Thread(1, OsThread, OsProcess, null);
        fixture.Thread(2, CaptureOsThread, 99, null);
        // ID 303 intentionally collides with a CLR event; an unrelated provider must not use that schema.
        fixture.Metadata(1, "Opaque", provider: "Unrelated.Provider", eventId: 303, version: 0);
        byte[] first = [0xff, 0x00, 0x80, 0xc3, 0xa9, 0x7f, 0x01];
        byte[] second = [0x11, 0x22, 0x33];
        fixture.Events(compressed, new Row(Payload: first),
            new Row(Sequence: 2, Timestamp: 1002, Payload: second),
            new Row(Sequence: 3, Timestamp: 1003));
        var trace = await ReadTraceAsync(fixture.Finish(), 1);
        Assert.Multiple(() =>
        {
            Assert.That(trace.Events, Has.Count.EqualTo(3));
            Assert.That(trace.Events[0].Payload.Keys, Is.EqualTo(new[] { "RawPayload" }));
            Assert.That(trace.Events[0].Payload.GetValueOrDefault("RawPayload"), Is.TypeOf<byte[]>().And.EqualTo(first));
            Assert.That(trace.Events[1].Payload.GetValueOrDefault("RawPayload"), Is.TypeOf<byte[]>().And.EqualTo(second));
            Assert.That(trace.Events[2].Payload, Is.Empty);
            Assert.That(trace.Events[0].Metadata.EventName, Is.EqualTo("Opaque"));
            Assert.That(trace.Events[0].Metadata.ProviderName, Is.EqualTo("Unrelated.Provider"));
            Assert.That(trace.EventMetadata.Any(m => m.MetadataId == 1 && m.FieldDefinitions.Count == 0), Is.True,
                "the input metadata has no schema; synthesized RawPayload metadata is allowed");
            Assert.That(trace.Events[0].Metadata.FieldDefinitions, Has.Count.EqualTo(1));
            Assert.That(trace.Events[0].Metadata.FieldDefinitions.Select(f => f.Name), Is.EqualTo(new[] { "RawPayload" }),
                "the synthetic field must be discoverable without borrowing the unrelated CLR schema");
        });
    }

    [TestCase(4, false)]
    [TestCase(4, true)]
    [TestCase(8, false)]
    [TestCase(8, true)]
    public async Task SchemaLessAllocationSample_UsesTracePointerWidthButKeepsSizes64Bit(int pointerSize, bool compressed)
    {
        var fixture = new V6TraceFixture(pointerSize);
        fixture.Thread(1, OsThread, OsProcess, null);
        fixture.Thread(2, CaptureOsThread, 99, null);
        fixture.Metadata(1, "", provider: "Microsoft-Windows-DotNETRuntime", eventId: 303, version: 0);
        ulong typeId = pointerSize == 4 ? 0xf1234567UL : 0xf123456789abcdefUL;
        ulong address = pointerSize == 4 ? 0x89abcdefUL : 0x89abcdef01234567UL;
        fixture.Events(compressed, new Row(Payload: Bytes(b =>
        {
            b.U32(1); b.U16(2); b.Pointer(typeId, pointerSize); b.Utf16("Allocated雪😀");
            b.Pointer(address, pointerSize); b.U64(0xfedcba9876543210); b.U64(0x8123456789abcdef);
        })));
        var trace = await ReadTraceAsync(fixture.Finish(), 1);
        var evt = trace.Events.Single();
        Assert.Multiple(() =>
        {
            Assert.That(trace.Metadata.PointerSize, Is.EqualTo(pointerSize));
            Assert.That(evt.Metadata.EventName, Is.EqualTo("AllocationSampled"));
            Assert.That(evt.Payload.GetValueOrDefault("AllocationKind"), Is.EqualTo(1u));
            Assert.That(evt.Payload.GetValueOrDefault("ClrInstanceID"), Is.EqualTo((ushort)2));
            // Pointer values may be boxed as uint or normalized to ulong, but must retain the high bit.
            Assert.That(evt.Payload.GetValueOrDefault("TypeID"), Is.EqualTo(typeId));
            Assert.That(evt.Payload.GetValueOrDefault("TypeName"), Is.EqualTo("Allocated雪😀"));
            Assert.That(evt.Payload.GetValueOrDefault("Address"), Is.EqualTo(address));
            Assert.That(evt.Payload.GetValueOrDefault("ObjectSize"), Is.TypeOf<ulong>().And.EqualTo(0xfedcba9876543210UL));
            Assert.That(evt.Payload.GetValueOrDefault("SampledByteOffset"), Is.TypeOf<ulong>().And.EqualTo(0x8123456789abcdefUL));
            Assert.That(evt.Payload, Has.Count.EqualTo(7));
        });
    }

}
