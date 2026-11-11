using System.Buffers;
using NetTrace.FastSerializer;
using static NetTrace.Test.V6TraceFixture;

namespace NetTrace.Test;

[TestFixture]
public class RuntimePayloadReaderTest
{
    [Test]
    public void Candidates_AreCachedInOriginalOrderAndShareNormalizedFields()
    {
        RuntimePayloadReader reader = new(new EventPayloadReader());
        var exact = reader.GetCandidates(1, 1, 8);
        var absent = reader.GetCandidates(1, 99, 8);
        Assert.Multiple(() =>
        {
            Assert.That(exact.Select(s => s.Name), Is.EqualTo(new[] { "GCStart_V1", "GCStart_V2", "GCStart" }));
            Assert.That(absent.Select(s => s.Name), Is.EqualTo(new[] { "GCStart_V2", "GCStart_V1", "GCStart" }));
            Assert.That(reader.GetCandidates(1, 1, 8), Is.SameAs(exact));
            Assert.That(absent[1].Fields, Is.SameAs(exact[0].Fields));
            Assert.That(reader.GetCandidates(1, 1, 4), Is.Not.SameAs(exact));
            Assert.That(reader.GetCandidates(int.MaxValue, 0, 8), Is.Empty);
        });
    }

    [Test]
    public void FixedAndMinimumBounds_DoNotAllocateOnWarmRejectedPayloads()
    {
        RuntimePayloadReader reader = new(new EventPayloadReader());
        var fixedSchemas = reader.GetCandidates(1, 0, 8);
        var variable = reader.GetCandidates(80, 1, 8)[0];
        Assert.Multiple(() =>
        {
            Assert.That(fixedSchemas.Select(s => s.FixedSize), Is.EqualTo(new long?[] { 8, 26, 18 }));
            Assert.That(fixedSchemas[0].AcceptsSize(7), Is.False);
            Assert.That(fixedSchemas[0].AcceptsSize(9), Is.False);
            Assert.That(variable.FixedSize, Is.Null);
            Assert.That(variable.MinimumSize, Is.EqualTo(20));
            Assert.That(variable.AcceptsSize(19), Is.False);
            Assert.That(variable.AcceptsSize(100), Is.True);
        });
        ReadOnlySequence<byte> shortFixed = new(new byte[7]);
        ReadOnlySequence<byte> shortVariable = new(new byte[1]);
        for (int i = 0; i < 100; i++)
        {
            reader.TryRead(shortFixed, 1, 0, 8, out _, out _);
            reader.TryRead(shortVariable, 80, 1, 8, out _, out _);
        }
        long before = GC.GetAllocatedBytesForCurrentThread();
        for (int i = 0; i < 1000; i++)
        {
            reader.TryRead(shortFixed, 1, 0, 8, out _, out _);
            reader.TryRead(shortVariable, 80, 1, 8, out _, out _);
        }
        Assert.That(GC.GetAllocatedBytesForCurrentThread() - before, Is.Zero);
    }

    [TestCase(4)]
    [TestCase(8)]
    public void NestedPointerLayouts_NormalizeWidthAndNativePadding(int pointerSize)
    {
        RuntimePayloadReader reader = new(new EventPayloadReader());
        var bytes = Bytes(b =>
        {
            b.U16(7); b.U16(1);
            b.U32(11); b.U32(12); b.U32(13);
            if (pointerSize == 8) b.U32(0xdeadbeef);
            b.Pointer(0xfedcba98, pointerSize); b.Pointer(15, pointerSize);
        });
        Assert.That(reader.TryRead(Segmented(bytes), 208, 0, pointerSize, out var schema, out var payload), Is.True);
        var fields = schema!.Fields[2].SubFieldDefinitions!;
        var values = (object[])payload!["Values"];
        var value = (IReadOnlyDictionary<string, object>)values[0];
        Assert.Multiple(() =>
        {
            Assert.That(fields.Any(f => f.AlignPointers || f.TypeCode == NetTraceTypeCode.Pointer), Is.False);
            Assert.That(fields.Count(f => f.TypeCode == NetTraceTypeCode.Padding32), Is.EqualTo(pointerSize == 8 ? 1 : 0));
            Assert.That(value.Keys, Is.EqualTo(new[] { "TimePlan", "TimeCompact", "TimeRelocate", "TotalRefs", "ZeroRefs" }));
            Assert.That(value["TotalRefs"], Is.EqualTo(0xfedcba98UL));
            Assert.That(value["TotalRefs"].GetType(), Is.EqualTo(pointerSize == 4 ? typeof(uint) : typeof(ulong)));
        });
    }

    [TestCase(false)]
    [TestCase(true)]
    public void EqualLengthVariablePayloads_SelectByDecodeNotLength(bool segmented)
    {
        RuntimePayloadReader reader = new(new EventPayloadReader());
        // V0 has a longer string, V1 has an instance-id suffix: identical payload lengths
        // but different layouts. Also exercise recovery from an earlier partial decode.
        var v0 = Bytes(b =>
        {
            b.U64(1); b.U64(2); b.U64(3); b.U32(4); b.U32(5); b.U32(6);
            b.Utf16("NS"); b.Utf16("Long"); b.Utf16("Sig");
        });
        var v1 = Bytes(b =>
        {
            b.U64(1); b.U64(2); b.U64(3); b.U32(4); b.U32(5); b.U32(6);
            b.Utf16("NS"); b.Utf16("Len"); b.Utf16("Sig"); b.U16(7);
        });
        Assert.That(v0.Length, Is.EqualTo(v1.Length));
        for (int i = 0; i < 3; i++)
        {
            Assert.That(reader.TryRead(Buffer(v0), 143, 0, 8, out var first, out var firstPayload), Is.True);
            Assert.That(reader.TryRead(Buffer(v1), 143, 0, 8, out var second, out var secondPayload), Is.True);
            Assert.Multiple(() =>
            {
                Assert.That(first!.Name, Is.EqualTo("MethodLoadVerbose"));
                Assert.That(second!.Name, Is.EqualTo("MethodLoadVerbose_V1"));
                Assert.That(firstPayload!["MethodName"], Is.EqualTo("Long"));
                Assert.That(secondPayload!["MethodName"], Is.EqualTo("Len"));
                Assert.That(secondPayload["ClrInstanceID"], Is.EqualTo(7));
            });
        }
        ReadOnlySequence<byte> Buffer(byte[] bytes) => segmented ? Segmented(bytes) : new(bytes);
    }

    [TestCase(4)]
    [TestCase(8)]
    public void AllManifestBounds_PreserveUnfilteredTrialResults(int pointerSize)
    {
        RuntimePayloadReader reader = new(new EventPayloadReader());
        EventPayloadReader baseline = new();
        // Include every known version plus an absent version; malformed variants must
        // remain rejected (or pick the same older schema), never silently length-selected.
        for (int eventId = 0; eventId <= 300; eventId++)
        {
            var manifest = RuntimeEventSchema.Candidates(eventId);
            foreach (int declared in manifest.Select(s => s.Version).Append(-1))
            {
                var ordered = reader.GetCandidates(eventId, declared, pointerSize);
                foreach (var candidate in ordered)
                {
                    byte[] full = ZeroPayload(candidate.Fields);
                    foreach (var bytes in new[] { full, full.Length == 0 ? new byte[] { 1 } : full[..^1], full.Concat(new byte[] { 0xff }).ToArray() })
                    {
                        var buffer = Segmented(bytes);
                        var expected = UnfilteredRead(baseline, buffer, ordered);
                        bool success = reader.TryRead(buffer, eventId, declared, pointerSize, out var actual, out _);
                        Assert.That(success, Is.EqualTo(expected != null), $"event {eventId}, declared {declared}, {candidate.Name}, length {bytes.Length}");
                        Assert.That(actual, Is.SameAs(expected));
                    }
                }
            }
        }
    }

    [TestCase(false, 4)]
    [TestCase(false, 8)]
    [TestCase(true, 4)]
    [TestCase(true, 8)]
    public async Task NetTraceIntegration_SharesOutputSchemaAndPreservesRawFallback(bool compressed, int pointerSize)
    {
        var fixture = new V6TraceFixture(pointerSize);
        fixture.Thread(1, 10, 42, "worker");
        fixture.Thread(2, 20, 42, "capture");
        fixture.Metadata(1, "Unspecified", provider: KnownEvent.RuntimeProvider, eventId: 31, version: 0);
        fixture.Metadata(2, "Unknown", provider: KnownEvent.RuntimeProvider, eventId: 9999, version: 0);
        var valid = Bytes(b => { b.Pointer(0xfedcba98, pointerSize); b.U16(7); });
        var trailing = valid.Concat(new byte[] { 0xff }).ToArray();
        fixture.Events(compressed, new Row(Payload: valid),
            new Row(Sequence: 2, Timestamp: 1002, Payload: trailing),
            new Row(Sequence: 3, Timestamp: 1003, Payload: valid),
            new Row(Metadata: 2, Sequence: 4, Timestamp: 1004, Payload: [0x01, 0x02, 0x03]),
            new Row(Metadata: 2, Sequence: 5, Timestamp: 1005));
        using MemoryStream stream = new(fixture.Finish());
        var trace = await new NetTraceReader(stream).ReadFullTraceAsync().WaitAsync(TimeSpan.FromSeconds(10));
        Assert.Multiple(() =>
        {
            Assert.That(trace.Events, Has.Count.EqualTo(5));
            Assert.That(trace.Events[0].Metadata.EventName, Is.EqualTo("DestroyGCHandle"));
            Assert.That(trace.Events[0].Metadata, Is.SameAs(trace.Events[2].Metadata));
            Assert.That(trace.Events[0].Metadata.FieldDefinitions, Is.SameAs(trace.Events[2].Metadata.FieldDefinitions));
            Assert.That(trace.Events[0].Payload["HandleID"].GetType(), Is.EqualTo(pointerSize == 4 ? typeof(uint) : typeof(ulong)));
            Assert.That(trace.Events[1].Payload["RawPayload"], Is.EqualTo(trailing));
            Assert.That(trace.Events[3].Payload["RawPayload"], Is.EqualTo(new byte[] { 1, 2, 3 }));
            Assert.That(trace.Events[4].Payload, Is.Empty);
            Assert.That(trace.Events[4].Metadata.FieldDefinitions, Is.Empty);
            Assert.That(trace.EventMetadata.Count(m => m.EventId == 31 && m.EventName == "DestroyGCHandle"), Is.EqualTo(1));
        });
    }

    [Test]
    public void EmptyKnownSchema_AndMalformedCountsOrStrings_KeepFullConsumption()
    {
        RuntimePayloadReader reader = new(new EventPayloadReader());
        Assert.That(reader.TryRead(ReadOnlySequence<byte>.Empty, 7, 0, 8, out var empty, out var payload), Is.True);
        Assert.That(empty!.Name, Is.EqualTo("GCRestartEEBegin"));
        Assert.That(payload, Is.Empty);
        var invalidCount = Bytes(b => { b.U16(7); b.U16(1); });
        var overflowCount = Bytes(b => { b.Utf16(""); b.U32(uint.MaxValue); b.U16(7); });
        var unterminated = Bytes(b => { b.U64(1); b.U64(2); b.Utf16(""); b.U16(7); });
        Assert.Multiple(() =>
        {
            Assert.That(reader.TryRead(new(invalidCount), 208, 0, 8, out _, out _), Is.False);
            Assert.That(reader.TryRead(new(overflowCount), 39, 0, 8, out _, out _), Is.False);
            Assert.That(reader.TryRead(new(unterminated), 33, 0, 8, out _, out _), Is.False);
            Assert.That(reader.TryRead(new(new byte[] { 1 }), 7, 0, 8, out _, out _), Is.False);
        });
    }

    private static RuntimePayloadReader.Candidate? UnfilteredRead(EventPayloadReader reader, ReadOnlySequence<byte> buffer,
        RuntimePayloadReader.Candidate[] candidates)
    {
        foreach (var candidate in candidates)
        {
            FastSerializerSequenceReader cursor = new(buffer, 0);
            try
            {
                reader.ReadObject(ref cursor, candidate.Fields, buffer);
                if (cursor.End) return candidate;
            }
            catch (Exception e) when (e is EndOfStreamException or ArgumentException or KeyNotFoundException
                or OverflowException or InvalidDataException)
            {
            }
        }
        return null;
    }

    private static byte[] ZeroPayload(IReadOnlyList<EventFieldDefinition> fields) => Bytes(b =>
    {
        foreach (var field in fields)
        {
            if (field.CountFieldName != null) continue;
            switch (field.TypeCode)
            {
                case NetTraceTypeCode.Object: b.Raw(ZeroPayload(field.SubFieldDefinitions!)); break;
                case NetTraceTypeCode.Byte: b.U8(0); break;
                case NetTraceTypeCode.UInt16: case NetTraceTypeCode.NullTerminatedUtf16String: b.U16(0); break;
                case NetTraceTypeCode.Int32: case NetTraceTypeCode.UInt32: case NetTraceTypeCode.Boolean32:
                case NetTraceTypeCode.Padding32: b.U32(0); break;
                case NetTraceTypeCode.UInt64: case NetTraceTypeCode.Double: b.U64(0); break;
                case NetTraceTypeCode.Guid: b.Raw(new byte[16]); break;
                default: throw new NotSupportedException(field.TypeCode.ToString());
            }
        }
    });

    private static ReadOnlySequence<byte> Segmented(byte[] bytes)
    {
        if (bytes.Length < 2) return new(bytes);
        // An odd split also exercises UTF16 terminators across segment boundaries.
        int split = bytes.Length / 2 | 1;
        var first = new Segment(bytes.AsMemory(0, split));
        var last = first.Append(bytes.AsMemory(split));
        return new(first, 0, last, last.Memory.Length);
    }

    private sealed class Segment : ReadOnlySequenceSegment<byte>
    {
        public Segment(ReadOnlyMemory<byte> memory) => Memory = memory;
        public Segment Append(ReadOnlyMemory<byte> memory)
        {
            var next = new Segment(memory) { RunningIndex = RunningIndex + Memory.Length };
            Next = next;
            return next;
        }
    }
}
