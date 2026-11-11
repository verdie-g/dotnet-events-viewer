using static NetTrace.Test.V6TraceFixture;
using System.Diagnostics.Tracing;

namespace NetTrace.Test;

[TestFixture]
public class NetTraceReaderV6Test
{
    private static readonly Guid Activity = new("00112233-4455-6677-8899-aabbccddeeff");
    private static readonly Guid Related = new("fedcba98-7654-3210-0123-456789abcdef");
    private static readonly Guid TraceId = new("01234567-89ab-cdef-0123-456789abcdef");
    private const long OsThread = 0x100000123;
    private const long CaptureOsThread = 0x200000456;
    private const long OsProcess = 0x300000789;

    [TestCase(false)]
    [TestCase(true)]
    public async Task LabelKinds1Through10_EndFlagAndMultipleLists_OverrideOnlySelectedMetadata(bool compressed)
    {
        var fixture = BasicFixture();
        fixture.Labels(1,
            [Label(1, b => b.Guid(Activity)), Label(2, b => b.Guid(Related)),
             Label(3, b => b.Guid(TraceId)), Label(4, b => b.U64(ulong.MaxValue - 7)),
             Label(5, b => { b.Text("clé雪"); b.Text("välue😀"); }),
             Label(6, b => { b.Text("signed"); b.ZigZag(long.MinValue); }),
             Label(7, b => b.U8(2)), Label(8, b => b.U64(0x8000000000000042)),
             Label(9, b => b.U8(2)), Label(10, b => b.U8(9))],
            [Label(5, b => { b.Text("second"); b.Text("separate list"); })],
            [Label(7, b => b.U8(0)), Label(8, b => b.U64(0)),
             Label(9, b => b.U8(0)), Label(10, b => b.U8(0))]);
        fixture.Events(compressed,
            new Row(LabelId: 1), new Row(Sequence: 2, Timestamp: 1002, LabelId: 1),
            new Row(Sequence: 3, Timestamp: 1003, LabelId: 2),
            new Row(Sequence: 4, Timestamp: 1004),
            new Row(Sequence: 5, Timestamp: 1005, LabelId: 3));

        var trace = await ReadTraceAsync(fixture.Finish());
        var first = trace.Events[0];
        var secondList = trace.Events[2];
        var unlabeled = trace.Events[3];
        var zeroOverrides = trace.Events[4];
        Assert.Multiple(() =>
        {
            Assert.That(trace.Events, Has.Count.EqualTo(5));
            Assert.That(first.ActivityId, Is.EqualTo(Activity));
            Assert.That(first.RelatedActivityId, Is.EqualTo(Related));
            Assert.That(first.TraceId, Is.EqualTo(TraceId));
            Assert.That(first.SpanId, Is.EqualTo(ulong.MaxValue - 7));
            Assert.That(first.Labels, Has.Count.EqualTo(2));
            Assert.That(first.Labels["clé雪"], Is.EqualTo("välue😀"));
            Assert.That(first.Labels["signed"], Is.TypeOf<long>().And.EqualTo(long.MinValue));
            Assert.That(first.Metadata.Opcode, Is.EqualTo(EventOpcode.Stop));
            Assert.That(first.Metadata.Keywords, Is.EqualTo(unchecked((EventKeywords)0x8000000000000042UL)));
            Assert.That(first.Metadata.Level, Is.EqualTo(EventLevel.Error));
            Assert.That(first.Metadata.Version, Is.EqualTo(9));
            Assert.That(trace.Events[1].Metadata, Is.SameAs(first.Metadata), "label-specific metadata should be cached");
            Assert.That(first.Metadata.EventName, Is.EqualTo("Synthetic"));
            Assert.That(first.Metadata.ProviderName, Is.EqualTo("Synthetic.Provider"));
            Assert.That(secondList.Labels, Has.Count.EqualTo(1));
            Assert.That(secondList.Labels["second"], Is.EqualTo("separate list"));
            Assert.That(secondList.ActivityId, Is.EqualTo(Guid.Empty));
            Assert.That(secondList.RelatedActivityId, Is.EqualTo(Guid.Empty));
            Assert.That(secondList.TraceId, Is.Null);
            Assert.That(secondList.SpanId, Is.Null);
            Assert.That(secondList.Metadata, Is.SameAs(unlabeled.Metadata));
            Assert.That(unlabeled.Labels, Is.Empty);
            Assert.That(unlabeled.Metadata.Opcode, Is.EqualTo(EventOpcode.Start));
            Assert.That(unlabeled.Metadata.Keywords, Is.EqualTo((EventKeywords)0x1234));
            Assert.That(unlabeled.Metadata.Level, Is.EqualTo(EventLevel.Informational));
            Assert.That(unlabeled.Metadata.Version, Is.EqualTo(3));
            Assert.That(zeroOverrides.Metadata.Opcode, Is.EqualTo(EventOpcode.Info));
            Assert.That(zeroOverrides.Metadata.Keywords, Is.EqualTo(EventKeywords.None));
            Assert.That(zeroOverrides.Metadata.Level, Is.EqualTo(EventLevel.LogAlways));
            Assert.That(zeroOverrides.Metadata.Version, Is.Zero);
            Assert.That(trace.EventMetadata, Has.Count.EqualTo(3));
        });
    }

    [TestCase(false)]
    [TestCase(true)]
    public async Task ThreadIndexes_MapToRealOsThreadsAndEventProcess_NotCaptureProcess(bool compressed)
    {
        var fixture = BasicFixture();
        fixture.Thread(3, 987, null, null);
        fixture.Events(compressed, new Row(Processor: 17),
            new Row(Sequence: 2, Timestamp: 1002, Thread: 3));
        var trace = await ReadTraceAsync(fixture.Finish());
        Assert.Multiple(() =>
        {
            Assert.That(trace.Metadata.Date, Is.EqualTo(new DateTime(2026, 1, 2, 3, 4, 5, 6)));
            Assert.That(trace.Metadata.PointerSize, Is.EqualTo(8));
            Assert.That(trace.Metadata.ProcessId, Is.EqualTo(42));
            Assert.That(trace.Metadata.NumberOfProcessors, Is.EqualTo(16));
            Assert.That(trace.Metadata.CpuSamplingRate, Is.EqualTo(1000));
            Assert.That(trace.Events[0].ThreadId, Is.EqualTo(OsThread));
            Assert.That(trace.Events[0].CaptureThreadId, Is.EqualTo(CaptureOsThread));
            Assert.That(trace.Events[0].ProcessId, Is.EqualTo(OsProcess));
            Assert.That(trace.Events[0].ThreadName, Is.EqualTo("worker雪😀"));
            Assert.That(trace.Events[0].ProcessorNumber, Is.EqualTo(17));
            Assert.That(trace.Events[0].TimeStamp, Is.EqualTo(1));
            Assert.That(trace.Events[1].ThreadId, Is.EqualTo(987));
            Assert.That(trace.Events[1].ProcessId, Is.EqualTo(42), "missing process uses trace process");
            Assert.That(trace.Events[1].ThreadName, Is.Null);
        });
    }

    [Test]
    public async Task CompressedAndUncompressedEvents_AreEquivalent_WithInheritedAndChangedHeaderFields()
    {
        Row[] rows =
        [
            new(Payload: Bytes(b => b.I32(10)), Stack: 1, LabelId: 1),
            new(Sequence: 2, Timestamp: 1002, Payload: Bytes(b => b.I32(20)), Stack: 1, LabelId: 1),
            new(Sequence: 7, Timestamp: 1100, Thread: 2, Capture: 1, Processor: 4,
                Metadata: 2, Payload: Bytes(b => { b.I32(30); b.I32(40); })),
            new(Sequence: 8, Timestamp: 1100, Thread: 2, Capture: 1, Processor: 4,
                Metadata: 2, Payload: Bytes(b => { b.I32(50); b.I32(60); })),
            new(Sequence: 9, Timestamp: 1200, Payload: Bytes(b => b.I32(70)), LabelId: 1)
        ];
        V6TraceFixture Build(bool compressed)
        {
            var fixture = BasicFixture([Field("Value", 9)]);
            fixture.Metadata(2, "Pair", [Field("Value", 9), Field("Other", 9)]);
            fixture.Stack(1, 0x1001, 0x2002);
            fixture.Labels(1, [Label(1, b => b.Guid(Activity))]);
            fixture.Events(compressed, rows);
            // Compression state starts at zero again at each EventBlock.
            fixture.Events(compressed, new Row(Sequence: 20, Timestamp: 1300, Payload: Bytes(b => b.I32(80))));
            return fixture;
        }
        var plain = await ReadTraceAsync(Build(false).Finish());
        var compressed = await ReadTraceAsync(Build(true).Finish());
        Assert.That(compressed.Events, Has.Count.EqualTo(6));
        for (int i = 0; i < plain.Events.Count; i++)
        {
            var expected = plain.Events[i];
            var actual = compressed.Events[i];
            Assert.Multiple(() =>
            {
                Assert.That(actual.Index, Is.EqualTo(expected.Index));
                Assert.That(actual.SequenceNumber, Is.EqualTo(expected.SequenceNumber));
                Assert.That(actual.ThreadId, Is.EqualTo(expected.ThreadId));
                Assert.That(actual.CaptureThreadId, Is.EqualTo(expected.CaptureThreadId));
                Assert.That(actual.ProcessId, Is.EqualTo(expected.ProcessId));
                Assert.That(actual.ProcessorNumber, Is.EqualTo(expected.ProcessorNumber));
                Assert.That(actual.ThreadName, Is.EqualTo(expected.ThreadName));
                Assert.That(actual.StackTrace.Addresses, Is.EqualTo(expected.StackTrace.Addresses));
                Assert.That(actual.TimeStamp, Is.EqualTo(expected.TimeStamp));
                Assert.That(actual.ActivityId, Is.EqualTo(expected.ActivityId));
                Assert.That(actual.RelatedActivityId, Is.EqualTo(expected.RelatedActivityId));
                Assert.That(actual.TraceId, Is.EqualTo(expected.TraceId));
                Assert.That(actual.SpanId, Is.EqualTo(expected.SpanId));
                Assert.That(actual.Labels, Is.EquivalentTo(expected.Labels));
                // Record equality compares the FieldDefinitions array by identity across readers.
                Assert.That(actual.Metadata with { FieldDefinitions = Array.Empty<EventFieldDefinition>() },
                    Is.EqualTo(expected.Metadata with { FieldDefinitions = Array.Empty<EventFieldDefinition>() }));
                Assert.That(actual.Metadata.FieldDefinitions, Is.EqualTo(expected.Metadata.FieldDefinitions));
                Assert.That(actual.Payload, Is.EquivalentTo(expected.Payload));
                Assert.That(actual.SequenceNumber, Is.EqualTo(i == 5 ? 20 : rows[i].Sequence));
            });
        }
        Assert.That(compressed.Events.Select(e => e.Index), Is.EqualTo(new[] { 0, 1, 2, 3, 4, 5 }),
            "equal timestamps retain file order");
    }

    [TestCase(false)]
    [TestCase(true)]
    public async Task SequencePoint_ReusesStackIdsButZeroRemainsEmpty_AndResetsLabelThreadMetadataCaches(bool compressed)
    {
        var fixture = BasicFixture();
        fixture.Stack(1, 0x1111);
        fixture.Labels(1, [Label(1, b => b.Guid(Activity)), Label(10, b => b.U8(9))]);
        fixture.Events(compressed, new Row(Stack: 1, LabelId: 1));
        fixture.SequencePoint(3);
        fixture.Thread(1, 5555, 6666, "new worker");
        fixture.Thread(2, 7777, 8888, "new capture");
        fixture.Metadata(1, "AfterReset");
        fixture.Labels(1, [Label(1, b => b.Guid(Related)), Label(10, b => b.U8(4))]);
        fixture.Stack(1, 0x2222);
        fixture.Events(compressed, new Row(Sequence: 2, Timestamp: 1002),
            new Row(Sequence: 3, Timestamp: 1003, Stack: 1, LabelId: 1));
        fixture.SequencePoint(0);
        fixture.Events(compressed, new Row(Sequence: 4, Timestamp: 1004));
        var trace = await ReadTraceAsync(fixture.Finish());
        Assert.Multiple(() =>
        {
            Assert.That(trace.Events[0].StackTrace.Addresses, Is.EqualTo(new ulong[] { 0x1111 }));
            Assert.That(trace.Events[2].StackTrace.Addresses, Is.EqualTo(new ulong[] { 0x2222 }));
            Assert.That(trace.Events[2].StackTrace.Index, Is.Not.EqualTo(trace.Events[0].StackTrace.Index));
            Assert.That(trace.Events[1].StackTrace, Is.SameAs(StackTrace.Empty));
            Assert.That(trace.Events[3].StackTrace, Is.SameAs(StackTrace.Empty));
            Assert.That(trace.Events[0].ActivityId, Is.EqualTo(Activity));
            Assert.That(trace.Events[0].Metadata.Version, Is.EqualTo(9));
            Assert.That(trace.Events[2].ActivityId, Is.EqualTo(Related));
            Assert.That(trace.Events[2].Metadata.Version, Is.EqualTo(4));
            Assert.That(trace.Events[2].Metadata.EventName, Is.EqualTo("AfterReset"));
            Assert.That(trace.Events[2].ThreadId, Is.EqualTo(5555));
            Assert.That(trace.Events[2].CaptureThreadId, Is.EqualTo(7777));
            Assert.That(trace.Events[2].ProcessId, Is.EqualTo(6666));
            Assert.That(trace.Events[2].ThreadName, Is.EqualTo("new worker"));
            Assert.That(trace.Events[1].ActivityId, Is.EqualTo(Guid.Empty));
            Assert.That(trace.Events[3].Metadata, Is.SameAs(trace.Events[1].Metadata));
        });
    }

    [TestCase(0, "label")]
    [TestCase(1, "thread")]
    [TestCase(2, "metadata")]
    public async Task SequencePoint_RejectsReferencesToFlushedTables(int flags, string table)
    {
        var fixture = BasicFixture();
        fixture.Labels(1, [Label(1, b => b.Guid(Activity))]);
        fixture.Events(false, new Row(LabelId: 1));
        fixture.SequencePoint(flags);
        fixture.Events(false, new Row(Sequence: 2, Timestamp: 1002, LabelId: table == "label" ? 1 : 0));
        var error = await ReadError(fixture.Finish());
        Assert.That(error, Is.InstanceOf<InvalidDataException>().Or.InstanceOf<KeyNotFoundException>(),
            $"stale {table} reference must not silently reuse a cached object");
    }

    [Test]
    public async Task RemoveThread_AllowsIndexReuseWithoutChangingEarlierEvents()
    {
        var fixture = BasicFixture();
        fixture.Events(true, new Row());
        fixture.Block(7, b => { b.Var(1); b.Var(1); });
        fixture.Thread(1, 321, 654, "replacement");
        fixture.Events(true, new Row(Sequence: 2, Timestamp: 1002));
        var trace = await ReadTraceAsync(fixture.Finish());
        Assert.Multiple(() =>
        {
            Assert.That(trace.Events[0].ThreadId, Is.EqualTo(OsThread));
            Assert.That(trace.Events[0].ProcessId, Is.EqualTo(OsProcess));
            Assert.That(trace.Events[1].ThreadId, Is.EqualTo(321));
            Assert.That(trace.Events[1].ProcessId, Is.EqualTo(654));
            Assert.That(trace.Events[1].ThreadName, Is.EqualTo("replacement"));
        });
    }

    [TestCase(1)]
    [TestCase(3)]
    [TestCase(7)]
    [TestCase(4095)]
    public async Task FragmentedStreams_ReadLittleEndianScalarsAndNonAsciiUtf16(int fragmentSize)
    {
        const string unicode = "雪Ωé😀\uffff\u8000";
        var fields = new[]
        {
            Field("Single", 13), Field("Double", 14), Field("SignedByte", 5), Field("ByteAfterSByte", 6),
            Field("Int16", 7), Field("UInt16", 8), Field("Int32", 9), Field("UInt32", 10),
            Field("Int64", 11), Field("UInt64", 12), Field("Utf16Char", 4), Field("Text", 18),
            Field("LegacyType23String", 23), Field("Boolean8", 26), Field("Boolean32", 3), Field("Tail", 9)
        };
        var payload = Bytes(b =>
        {
            // Literal IEEE754 little-endian bytes, not parser helpers or host-endian BitConverter output.
            b.Raw([0x00, 0x00, 0xa0, 0xbf]); // -1.25f
            b.Raw([0x18, 0x2d, 0x44, 0x54, 0xfb, 0x21, 0x09, 0xc0]); // -Math.PI
            b.U8(0xff); b.U8(0xac); b.I16(short.MinValue); b.U16(ushort.MaxValue);
            b.I32(int.MinValue); b.U32(uint.MaxValue); b.I64(long.MinValue); b.U64(ulong.MaxValue);
            b.U16('雪'); b.Utf16(unicode); b.ShortText("hé雪😀"); b.U8(1); b.I32(1); b.I32(0x12345678);
        });
        var fixture = BasicFixture(fields);
        fixture.Events(true, new Row(Payload: payload));
        var trace = await ReadTraceAsync(fixture.Finish(), fragmentSize);
        var values = trace.Events.Single().Payload;
        Assert.Multiple(() =>
        {
            Assert.That(values["Single"], Is.TypeOf<float>().And.EqualTo(-1.25f));
            Assert.That(values["Double"], Is.TypeOf<double>().And.EqualTo(-Math.PI));
            Assert.That(values["SignedByte"], Is.TypeOf<sbyte>().And.EqualTo((sbyte)-1));
            Assert.That(values["ByteAfterSByte"], Is.TypeOf<byte>().And.EqualTo((byte)0xac));
            Assert.That(values["Int16"], Is.TypeOf<short>().And.EqualTo(short.MinValue));
            Assert.That(values["UInt16"], Is.TypeOf<ushort>().And.EqualTo(ushort.MaxValue));
            Assert.That(values["Int32"], Is.TypeOf<int>().And.EqualTo(int.MinValue));
            Assert.That(values["UInt32"], Is.TypeOf<uint>().And.EqualTo(uint.MaxValue));
            Assert.That(values["Int64"], Is.TypeOf<long>().And.EqualTo(long.MinValue));
            Assert.That(values["UInt64"], Is.TypeOf<ulong>().And.EqualTo(ulong.MaxValue));
            Assert.That(values["Utf16Char"], Is.TypeOf<char>().And.EqualTo('雪'));
            Assert.That(values["Text"], Is.EqualTo(unicode));
            Assert.That(values["LegacyType23String"], Is.TypeOf<string>().And.EqualTo("hé雪😀"));
            Assert.That(values["Boolean8"], Is.True);
            Assert.That(values["Boolean32"], Is.True);
            Assert.That(values["Tail"], Is.EqualTo(0x12345678));
        });
    }

    [TestCase(0L)]
    [TestCase(-1L)]
    [TestCase(1L)]
    [TestCase(-65L)]
    [TestCase(8192L)]
    [TestCase(long.MinValue)]
    [TestCase(long.MaxValue)]
    public async Task SignedZigZagAndLargeUnsignedVarints_PreserveAll64Bits(long signed)
    {
        var fixture = BasicFixture([Field("Signed", 20), Field("Unsigned", 21), Field("Tail", 6)]);
        fixture.Events(true, new Row(Payload: Bytes(b => { b.ZigZag(signed); b.Var(ulong.MaxValue); b.U8(0x5a); })));
        var values = (await ReadTraceAsync(fixture.Finish(), 1)).Events.Single().Payload;
        Assert.Multiple(() =>
        {
            Assert.That(values["Signed"], Is.TypeOf<long>().And.EqualTo(signed));
            Assert.That(values["Unsigned"], Is.TypeOf<ulong>().And.EqualTo(ulong.MaxValue));
            Assert.That(values["Tail"], Is.EqualTo((byte)0x5a));
        });
    }

    [TestCase(false)]
    [TestCase(true)]
    public async Task DuplicateFieldNames_LastValueWinsAndReadOnlyDictionaryViewsAgreeAcrossEvents(bool compressed)
    {
        var fixture = BasicFixture([
            Field("Repeated", 9), Field("Tail", 6), Field("Repeated", 18),
            Field("Nested", new Shape(1, Fields: [Field("Value", 9), Field("Value", 18), Field("Last", 6)]))
        ]);
        byte[] Payload(int value, string text) => Bytes(b =>
        {
            b.I32(value); b.U8((byte)value); b.Utf16(text);
            b.I32(-value); b.Utf16($"nested-{text}"); b.U8((byte)(value + 1));
        });
        fixture.Events(compressed, new Row(Payload: Payload(10, "first雪")),
            new Row(Sequence: 2, Timestamp: 1002, Payload: Payload(20, "second😀")));
        var trace = await ReadTraceAsync(fixture.Finish(), 1);
        for (int i = 0; i < trace.Events.Count; i++)
        {
            IReadOnlyDictionary<string, object> payload = trace.Events[i].Payload;
            var nested = (IReadOnlyDictionary<string, object>)payload["Nested"];
            string text = i == 0 ? "first雪" : "second😀";
            byte tail = (byte)(i == 0 ? 10 : 20);
            var entries = payload.ToArray();
            var nonGenericEntries = ((System.Collections.IEnumerable)payload).Cast<KeyValuePair<string, object>>().ToArray();
            Assert.Multiple(() =>
            {
                Assert.That(payload.Count, Is.EqualTo(3), "count is unique names, not wire field count");
                Assert.That(payload["Repeated"], Is.TypeOf<string>().And.EqualTo(text));
                Assert.That(payload["Tail"], Is.EqualTo(tail), "overwritten fields must still consume their wire bytes");
                Assert.That(payload.ContainsKey("Repeated"), Is.True);
                Assert.That(payload.TryGetValue("Repeated", out var repeated), Is.True);
                Assert.That(repeated, Is.EqualTo(text));
                Assert.That(payload.ContainsKey("missing"), Is.False);
                Assert.That(payload.TryGetValue("missing", out _), Is.False);
                Assert.Throws<KeyNotFoundException>(() => _ = payload["missing"]);
                Assert.That(entries.Select(e => e.Key), Is.EquivalentTo(new[] { "Repeated", "Tail", "Nested" }));
                Assert.That(entries.Select(e => e.Key), Is.EqualTo(payload.Keys));
                Assert.That(entries.Select(e => e.Value), Is.EqualTo(payload.Values));
                Assert.That(entries.All(e => Equals(e.Value, payload[e.Key])), Is.True);
                Assert.That(nonGenericEntries, Is.EqualTo(entries));
                foreach (var dictionary in new[] { payload, nested })
                {
                    Assert.That(dictionary.Keys, Is.Not.InstanceOf<string[]>(),
                        "keys must not expose the schema's shared mutable name array");
                    if (dictionary.Keys is IList<string> keyList)
                    {
                        Assert.That(keyList.IsReadOnly, Is.True);
                        Assert.Throws<NotSupportedException>(() => keyList[0] = "tampered");
                    }
                    if (dictionary.Keys is System.Collections.IList untypedList)
                    {
                        Assert.That(untypedList.IsReadOnly, Is.True);
                        Assert.Throws<NotSupportedException>(() => untypedList[0] = "tampered");
                    }
                    if (dictionary.Keys is ICollection<string> keyCollection)
                    {
                        Assert.That(keyCollection.IsReadOnly, Is.True);
                        Assert.Throws<NotSupportedException>(() => keyCollection.Add("tampered"));
                    }
                }
                Assert.That(nested.Count, Is.EqualTo(2));
                Assert.That(nested["Value"], Is.EqualTo($"nested-{text}"));
                Assert.That(nested["Last"], Is.EqualTo((byte)(tail + 1)));
                Assert.That(nested.ToDictionary(e => e.Key, e => e.Value), Is.EquivalentTo(
                    new Dictionary<string, object> { ["Value"] = $"nested-{text}", ["Last"] = (byte)(tail + 1) }));
            });
        }
        Assert.That(trace.Events, Has.Count.EqualTo(2));
        Assert.That(trace.Events[0].Payload["Repeated"], Is.EqualTo("first雪"),
            "sharing a schema layout must not overwrite an earlier event's values");
    }

    [TestCase(false)]
    [TestCase(true)]
    public async Task Arrays_ReadFixedVariableNestedObjectsAndEmptyArrays(bool compressed)
    {
        var fixedPair = new Shape(22, new Shape(6), Count: 2);
        var fields = new[]
        {
            Field("Fixed", new Shape(22, new Shape(9), Count: 2)),
            Field("Variable", new Shape(19, new Shape(8))),
            Field("Nested", new Shape(19, fixedPair)),
            Field("Objects", new Shape(19, new Shape(1, Fields: [Field("Id", 9), Field("Name", 18)]))),
            Field("Empty", new Shape(19, new Shape(12))), Field("Tail", 6)
        };
        var fixture = BasicFixture(fields);
        fixture.Events(compressed, new Row(Payload: Bytes(b =>
        {
            b.I32(-7); b.I32(123); b.U16(3); b.U16(1); b.U16(256); b.U16(65535);
            b.U16(2); b.Raw([1, 2, 3, 4]); b.U16(2);
            b.I32(10); b.Utf16("雪"); b.I32(20); b.Utf16("😀"); b.U16(0); b.U8(0x55);
        })));
        var trace = await ReadTraceAsync(fixture.Finish(), 3);
        var values = trace.Events.Single().Payload;
        var objects = (object[])values["Objects"];
        Assert.Multiple(() =>
        {
            Assert.That(values["Fixed"], Is.EqualTo(new object[] { -7, 123 }));
            Assert.That(values["Variable"], Is.EqualTo(new object[] { (ushort)1, (ushort)256, ushort.MaxValue }));
            Assert.That(values["Nested"], Is.EqualTo(new object[]
                { new object[] { (byte)1, (byte)2 }, new object[] { (byte)3, (byte)4 } }));
            Assert.That(((IReadOnlyDictionary<string, object>)objects[0])["Id"], Is.EqualTo(10));
            Assert.That(((IReadOnlyDictionary<string, object>)objects[0])["Name"], Is.EqualTo("雪"));
            Assert.That(((IReadOnlyDictionary<string, object>)objects[1])["Name"], Is.EqualTo("😀"));
            Assert.That(values["Empty"], Is.Empty);
            Assert.That(values["Tail"], Is.EqualTo((byte)0x55));
            Assert.That(trace.EventMetadata[0].FieldDefinitions[0].ElementCount, Is.EqualTo(2));
            Assert.That(trace.EventMetadata[0].FieldDefinitions[2].ElementDefinition!.TypeCode,
                Is.EqualTo(NetTraceTypeCode.FixedLengthArray));
        });
    }

    [TestCase(false)]
    [TestCase(true)]
    public async Task RelLocAndDataLoc_UseByteSizesAndCorrectOrigins_WithoutMovingInlineCursor(bool compressed)
    {
        var locations = new Shape(1, Fields:
        [
            Field("Relative", new Shape(24, new Shape(8))),
            Field("Absolute", new Shape(25, new Shape(22, new Shape(8), Count: 2)))
        ]);
        var fixture = BasicFixture([Field("Prefix", 9), Field("Locations", locations), Field("Tail", 9)]);
        fixture.Events(compressed, new Row(Payload: Bytes(b =>
        {
            b.I32(123);
            b.U32((4u << 16) | 8u); // after relative field = 8; data at 16 => offset 8
            b.U32((8u << 16) | 20u); // absolute payload offset 20, two fixed pairs of UInt16
            b.I32(456); // inline tail must not be read from a location target
            b.U16(0x1234); b.U16(0xabcd);
            b.U16(1); b.U16(2); b.U16(3); b.U16(4);
        })));
        var values = (await ReadTraceAsync(fixture.Finish(), 1)).Events.Single().Payload;
        var nested = (IReadOnlyDictionary<string, object>)values["Locations"];
        Assert.Multiple(() =>
        {
            Assert.That(values["Prefix"], Is.EqualTo(123));
            Assert.That(nested["Relative"], Is.EqualTo(new object[] { (ushort)0x1234, (ushort)0xabcd }));
            Assert.That(nested["Absolute"], Is.EqualTo(new object[]
                { new object[] { (ushort)1, (ushort)2 }, new object[] { (ushort)3, (ushort)4 } }));
            Assert.That(values["Tail"], Is.EqualTo(456));
        });
    }

    [TestCase(24, 3u << 16)]
    [TestCase(25, 3u << 16)]
    [TestCase(24, (4u << 16) | 0xffffu)]
    [TestCase(25, (4u << 16) | 0xffffu)]
    public async Task MalformedLocationArrays_AreRejected(int kind, uint location)
    {
        var fixture = BasicFixture([Field("BadLocation", new Shape((byte)kind, new Shape(8)))]);
        fixture.Events(false, new Row(Payload: Bytes(b => b.U32(location))));
        var error = await ReadError(fixture.Finish());
        Assert.That(error, Is.InstanceOf<InvalidDataException>().Or.InstanceOf<ArgumentOutOfRangeException>());
    }

    [Test]
    public async Task MetadataExtensionsAndUnknownBlocks_AreSkipped_AndOptionalMetadataIsPreserved()
    {
        var fixture = new V6TraceFixture();
        fixture.Thread(1, OsThread, OsProcess, "worker");
        fixture.Thread(2, CaptureOsThread, 99, "capture");
        fixture.Metadata(1, "événement雪", [Field("値", 9)], provider: "Prövider雪", extended: true);
        fixture.Block(0x7f, b => b.Raw([1, 2, 3, 4, 5]));
        fixture.Events(false, new Row(Payload: Bytes(b => b.I32(321))));
        var trace = await ReadTraceAsync(fixture.Finish(), 1);
        var metadata = trace.Events.Single().Metadata;
        Assert.Multiple(() =>
        {
            Assert.That(metadata.ProviderName, Is.EqualTo("Prövider雪"));
            Assert.That(metadata.EventName, Is.EqualTo("événement雪"));
            Assert.That(metadata.ProviderGuid, Is.EqualTo(Activity));
            Assert.That(metadata.EventDescription, Is.EqualTo("description雪"));
            Assert.That(metadata.MessageTemplate, Is.EqualTo("value={値}"));
            Assert.That(trace.Events[0].Payload["値"], Is.EqualTo(321));
        });
    }

    [TestCase("missing-tag")]
    [TestCase("opcode")]
    [TestCase("level")]
    [TestCase("version")]
    [TestCase("keywords")]
    [TestCase("message-template")]
    [TestCase("description")]
    [TestCase("key-value")]
    [TestCase("provider-guid")]
    public async Task OptionalMetadataSize_CannotBorrowRequiredBytesFromOutsideBoundedSection(string corruption)
    {
        var fixture = new V6TraceFixture();
        fixture.Block(3, b =>
        {
            b.U16(0);
            var row = Bytes(r =>
            {
                r.Var(1); r.Text("Synthetic.Provider"); r.Var(123); r.Text("MalformedOptionalMetadata");
                r.U16(0); // no field descriptions
                if (corruption == "missing-tag") r.U16(1); // no bytes available, even for the tag
                else
                {
                    var optional = Bytes(o =>
                    {
                        switch (corruption)
                        {
                            case "opcode": o.U8(1); o.U8(1); break;
                            case "keywords": o.U8(3); o.U64(0x1234); break;
                            case "message-template": o.U8(4); o.Text("message雪"); break;
                            case "description": o.U8(5); o.Text("description雪"); break;
                            case "key-value": o.U8(6); o.Text("key"); o.Text("value雪"); break;
                            case "provider-guid": o.U8(7); o.Guid(Activity); break;
                            case "level": o.U8(8); o.U8(4); break;
                            case "version": o.U8(9); o.U8(3); break;
                            default: throw new ArgumentException(corruption);
                        }
                    });
                    // Keep scalar operands outside the section. For variable operands retain
                    // a partial string/Guid inside, with its completing byte in the row extension.
                    ushort declaredSize = (ushort)(corruption is "opcode" or "keywords" or "level" or "version"
                        ? 1 : optional.Length - 1);
                    r.Sized16(optional, declaredSize);
                }
            });
            b.Sized16(row);
            // A well-formed neighboring metadata row must not supply the missing byte.
            var next = Bytes(r =>
            {
                r.Var(2); r.Text("Synthetic.Provider"); r.Var(124); r.Text("NextMetadata"); r.U16(0); r.U16(0);
            });
            b.Sized16(next);
        });
        var error = await ReadError(fixture.Finish(), 1);
        Assert.That(error, Is.InstanceOf<InvalidDataException>().Or.InstanceOf<EndOfStreamException>(),
            "malformed optional metadata must be rejected at its declared boundary");
    }

    [TestCase("name-length")]
    [TestCase("name-bytes")]
    [TestCase("type")]
    [TestCase("array-element")]
    [TestCase("fixed-count")]
    [TestCase("object-count")]
    [TestCase("nested-field")]
    public async Task FieldDescriptionSize_RejectsPartialNameAndRecursiveTypesBeforeReadingFollowingBytes(string corruption)
    {
        var fixture = new V6TraceFixture();
        byte[] incomplete = corruption switch
        {
            "name-length" => [0x80], // unterminated UTF8 byte-count varint
            "name-bytes" => [3, (byte)'A'],
            "type" => [0], // complete empty name but no scalar type
            "array-element" => [0, 19],
            "fixed-count" => [0, 22, 6, 2], // UInt16 count missing its high byte
            "object-count" => [0, 1, 1], // UInt16 field count missing its high byte
            "nested-field" => [0, 1, 1, 0, 2, 0, 0], // nested description lacks scalar type
            _ => throw new ArgumentException(corruption)
        };
        byte[] completion = corruption switch
        {
            "name-length" => [0, 6],
            "name-bytes" => [(byte)'B', (byte)'C', 6],
            "type" or "array-element" or "nested-field" => [6],
            "fixed-count" => [0],
            "object-count" => [0, 2, 0, 0, 6], // complete one empty-named byte field
            _ => throw new ArgumentException(corruption)
        };
        fixture.Block(3, b =>
        {
            b.U16(0);
            b.Sized16(Bytes(r =>
            {
                r.Var(1); r.Text("Synthetic.Provider"); r.Var(123); r.Text("MalformedField");
                r.U16(1);
                // A late extent check could parse a complete description using completion.
                // A bounded field reader must fail before consulting any of those bytes.
                r.Sized16(incomplete);
                r.Raw(completion);
                r.U16(0); // optional metadata follows the field
            }));
        });
        fixture.Metadata(2, "NextMetadata", [Field("Next", 6)]);
        Assert.That(await ReadError(fixture.Finish(), 1), Is.TypeOf<EndOfStreamException>(),
            "required field bytes are absent inside FieldDescriptionSize, not invalid after parsing the next bytes");
    }

    [TestCase(false, "row-header")]
    [TestCase(true, "row-header")]
    [TestCase(false, "payload")]
    [TestCase(true, "payload")]
    [TestCase(false, "inherited-payload")]
    [TestCase(true, "inherited-payload")]
    public async Task EventRows_CannotReadHeaderOrPayloadPastDeclaredBlockIntoSibling(bool compressed, string corruption)
    {
        var fixture = BasicFixture();
        var row = new Row(Payload: [0x11, 0x22, 0x33, 0x44]);
        byte[] body = corruption == "inherited-payload"
            ? EventBlockBody(compressed, row, row with { Sequence = 2, Timestamp = 1002 })
            : EventBlockBody(compressed, row);
        int declaredLength = corruption == "row-header" ? 21 : body.Length - 1;
        fixture.Block(2, body[..declaredLength]);
        // A complete, valid sibling block is buffered after the truncated row. Its header
        // provides many bytes to an unbounded decoder; it must never complete this row.
        fixture.Block(0x7f, new byte[256]);
        fixture.Events(compressed, new Row(Sequence: 3, Timestamp: 1003));
        Assert.That(await ReadError(fixture.Finish()), Is.TypeOf<EndOfStreamException>(),
            "all required event-row bytes must come from the declared EventBlock body");
    }

    [TestCase(false, 3)]
    [TestCase(true, 3)]
    [TestCase(false, 19)]
    [TestCase(true, 19)]
    [TestCase(false, 24)]
    [TestCase(true, 24)]
    public async Task EventBlockHeader_CannotReadTimestampOrExtensionFromSibling(bool compressed, int declaredLength)
    {
        var fixture = BasicFixture();
        byte[] body = Bytes(b =>
        {
            b.U16(28); b.U16((ushort)(compressed ? 1 : 0));
            b.I64(1001); b.I64(1001); b.Raw(new byte[8]);
        });
        fixture.Block(2, body[..declaredLength]);
        fixture.Block(0x7f, new byte[256]);
        Assert.That(await ReadError(fixture.Finish()), Is.TypeOf<EndOfStreamException>(),
            "block-header scalars and HeaderSize extensions are bounded by the object body");
    }

    [TestCase("negative-size")]
    [TestCase("zero-size")]
    [TestCase("short-header")]
    [TestCase("payload-larger-than-event")]
    [TestCase("payload-smaller-than-event")]
    [TestCase("oversized-across-next-row")]
    public async Task UncompressedEventSize_RejectsInvalidHeaderPayloadSizesAndCrossRowExtent(string corruption)
    {
        var fixture = BasicFixture([Field("Value", 9)]);
        var firstPayload = Bytes(b => b.I32(0x12345678));
        var next = new Row(Sequence: 2, Timestamp: 1002, Payload: Bytes(b => b.I32(8765)));
        fixture.Block(2, b =>
        {
            b.U16(20); b.U16(0); b.I64(1001); b.I64(1002);
            int eventSize = corruption switch
            {
                "negative-size" => -1,
                "zero-size" => 0,
                "short-header" => 47,
                "payload-smaller-than-event" => 56,
                // 52 bytes for our actual header/payload, plus all 56 bytes of the next row.
                // This remains in the block: checking only block bounds would silently skip it.
                "oversized-across-next-row" => 108,
                _ => 52
            };
            V6TraceFixture.WriteUncompressedRow(b, new Row(Payload: firstPayload), eventSize,
                corruption == "payload-larger-than-event" ? 8 : 4);
            V6TraceFixture.WriteUncompressedRow(b, next);
        });
        var error = await ReadError(fixture.Finish(), 1);
        Assert.That(error, Is.InstanceOf<InvalidDataException>().Or.InstanceOf<EndOfStreamException>(),
            "EventSize must contain the 48-byte header plus exactly PayloadSize bytes, not the next event");
    }

    [TestCase(4, false)]
    [TestCase(4, true)]
    [TestCase(8, false)]
    [TestCase(8, true)]
    public async Task StackAddresses_UseTracePointerWidthWithoutSignExtensionOrCombiningFrames(int pointerSize, bool compressed)
    {
        var fixture = new V6TraceFixture(pointerSize);
        fixture.Thread(1, OsThread, OsProcess, null);
        fixture.Thread(2, CaptureOsThread, 99, null);
        fixture.Metadata(1, "StackWidths");
        ulong[] addresses = pointerSize == 4
            ? [0x81234567UL, 0xffffffffUL, 0x10203040UL]
            : [0x8123456789abcdefUL, ulong.MaxValue, 0x1020304050607080UL];
        fixture.Stack(1, addresses);
        fixture.Events(compressed, new Row(Stack: 1));
        var trace = await ReadTraceAsync(fixture.Finish(), 1);
        Assert.Multiple(() =>
        {
            Assert.That(trace.Metadata.PointerSize, Is.EqualTo(pointerSize));
            Assert.That(trace.Events.Single().StackTrace.Addresses, Is.EqualTo(addresses));
            Assert.That(trace.Events.Single().StackTrace.Frames, Has.Length.EqualTo(3));
        });
    }

    [TestCase(4)]
    [TestCase(8)]
    public async Task StackByteSize_NotMultipleOfPointerWidth_IsRejected(int pointerSize)
    {
        var fixture = new V6TraceFixture(pointerSize);
        fixture.Block(5, b =>
        {
            b.I32(1); b.I32(1); b.I32(pointerSize + 1); b.Raw(new byte[pointerSize + 1]);
        });
        Assert.That(await ReadError(fixture.Finish(), 1), Is.InstanceOf<InvalidDataException>());
    }

    [TestCase(0)]
    [TestCase(1)]
    [TestCase(7)]
    [TestCase(8)]
    [TestCase(11)]
    [TestCase(12)]
    [TestCase(15)]
    [TestCase(19)]
    public async Task TruncatedV6Header_ThrowsEndOfStream(int length)
    {
        var header = Bytes(b => { b.Raw("Nettrace"u8.ToArray()); b.I32(0); b.I32(6); b.I32(0); });
        Assert.That(await ReadError(header[..length], 1), Is.TypeOf<EndOfStreamException>());
    }

    [TestCase("magic")]
    [TestCase("version")]
    public async Task InvalidHeader_ThrowsInvalidData(string corruption)
    {
        var fixture = new V6TraceFixture().Finish();
        if (corruption == "magic") fixture[0] = 0;
        else fixture[12] = 5;
        Assert.That(await ReadError(fixture, 1), Is.TypeOf<InvalidDataException>());
    }

    [TestCase(0)]
    [TestCase(1)]
    [TestCase(2)]
    [TestCase(3)]
    public async Task MissingOrPartialEndOfStreamBlock_ThrowsEndOfStream(int footerBytes)
    {
        var fixture = new V6TraceFixture().Finish();
        Assert.That(await ReadError(fixture[..^(4 - footerBytes)], 3), Is.TypeOf<EndOfStreamException>());
    }

    [Test]
    public async Task TruncatedBlockPayload_ThrowsEndOfStream()
    {
        var fixture = BasicFixture([Field("Text", 18)]);
        fixture.Events(false, new Row(Payload: Bytes(b => b.Utf16("雪😀"))));
        var bytes = fixture.Finish();
        Assert.That(await ReadError(bytes[..^5], 1), Is.TypeOf<EndOfStreamException>());
    }

    [TestCase(false)]
    [TestCase(true)]
    public async Task TruncatedEventPayloadWithinCompleteBlock_ThrowsEndOfStream(bool compressed)
    {
        var fixture = BasicFixture([Field("Text", 18)]);
        fixture.Events(compressed, new Row(Payload: [0xea, 0x96])); // valid code unit, absent UTF16 terminator
        Assert.That(await ReadError(fixture.Finish(), 1), Is.TypeOf<EndOfStreamException>());
    }

    [Test]
    public async Task EndOfStream_StopsBeforeTrailingInvalidBlocksAndCancelsProducer()
    {
        var fixture = new V6TraceFixture();
        // Finish includes the four-byte EOS block; this is the trace block's end position.
        int traceBlockEnd = fixture.Finish().Length - 4;
        fixture.Block(0, _ => { });
        fixture.Block(8, b => { b.I32(1); b.I32(1); b.U8(0x8b); });
        int boundary = fixture.Finish().Length - 4;
        fixture.Block(0x7f, b => b.Raw(new byte[256 * 1024]));
        using var stream = new FragmentStream(fixture.Finish(), 64 * 1024, cancelAtBoundary: boundary);
        var reader = new NetTraceReader(stream).AddProgress(new InlineProgress(progress =>
        {
            if (progress.BytesRead == traceBlockEnd)
            {
                // The whole EOS and invalid label block are already in the pipe. Hold the
                // consumer before EOS until the next producer read is blocked at our gate.
                stream.BoundaryReached.Task.WaitAsync(ReadTimeout).GetAwaiter().GetResult();
            }
        }));
        Trace trace;
        try { trace = await reader.ReadFullTraceAsync().WaitAsync(ReadTimeout); }
        finally { stream.Abort(); }
        Assert.Multiple(() =>
        {
            Assert.That(trace.Events, Is.Empty, "the buffered invalid label block after EOS must not be parsed");
            Assert.That(stream.BoundaryReached.Task.IsCompletedSuccessfully, Is.True);
            Assert.That(stream.BoundaryReadCancelled, Is.True, "EOS must cancel the actual gated producer read");
            Assert.That(stream.LastReadToken.IsCancellationRequested, Is.True);
            Assert.That(stream.ActiveReads, Is.Zero);
            Assert.That(stream.BytesRead, Is.EqualTo(boundary), "the explicit gate, not scheduling, bounds read-ahead");
            Assert.That(stream.BytesRead, Is.LessThan(stream.Length));
        });
    }

    [TestCase(false)]
    [TestCase(true)]
    public async Task LargeKnownOrUnknownBlockBeforeEndOfStream_ExceedsPipePauseThresholdAndPreservesEvents(bool knownBlock)
    {
        var fixture = BasicFixture([Field("Value", 9)]);
        fixture.Metadata(2, "LargeOpaque", provider: "Unrelated.Provider", eventId: 9999, version: 0);
        fixture.Events(false, new Row(Payload: Bytes(b => b.I32(111))));
        // The parser waits for a whole block. Both forms must be consumable despite exceeding
        // the default Pipe pause threshold (64 KiB), not merely ignored after an early EOS.
        // Event block: 20-byte block header + 52-byte row header + payload = exactly 256 KiB.
        byte[] payload = new byte[256 * 1024 - 72];
        payload[0] = 0x81; payload[payload.Length / 2] = 0x42; payload[^1] = 0xfe;
        if (knownBlock)
            fixture.Events(false, new Row(Metadata: 2, Sequence: 2, Timestamp: 1002, Payload: payload));
        else
            fixture.Block(0x7f, b => b.Raw(new byte[256 * 1024]));
        fixture.Events(false, new Row(Sequence: 3, Timestamp: 1003, Payload: Bytes(b => b.I32(333))));
        using var stream = new FragmentStream(fixture.Finish(), 4096);
        var trace = await Read(stream); // WaitAsync(ReadTimeout) bounds each case at ten seconds.
        Assert.Multiple(() =>
        {
            Assert.That(trace.Events, Has.Count.EqualTo(knownBlock ? 3 : 2));
            Assert.That(trace.Events[0].Payload["Value"], Is.EqualTo(111));
            Assert.That(trace.Events[^1].Payload["Value"], Is.EqualTo(333));
            Assert.That(trace.Events[^1].SequenceNumber, Is.EqualTo(3));
            Assert.That(stream.BytesRead, Is.EqualTo(stream.Length));
            Assert.That(stream.ActiveReads, Is.Zero);
            if (knownBlock)
            {
                Assert.That(trace.Events[1].Metadata.EventName, Is.EqualTo("LargeOpaque"));
                Assert.That(trace.Events[1].Payload.GetValueOrDefault("RawPayload"), Is.TypeOf<byte[]>().And.EqualTo(payload));
            }
        });
    }

    [TestCase("unknown0")]
    [TestCase("unknown11")]
    [TestCase("unknown127")]
    [TestCase("unterminated")]
    [TestCase("missing-list")]
    [TestCase("short-guid")]
    [TestCase("short-string")]
    [TestCase("overflow-varint")]
    public async Task MalformedLabels_LargeFixtureTerminatesSafely_WithoutBlockedProducer(string corruption)
    {
        var fixture = new V6TraceFixture();
        fixture.Block(8, b =>
        {
            b.I32(1); b.I32(corruption == "missing-list" ? 2 : 1);
            switch (corruption)
            {
                case "unknown0": b.U8(0x80); break;
                case "unknown11": b.U8(0x8b); break;
                case "unknown127": b.U8(0xff); break;
                case "unterminated": b.U8(10); b.U8(1); break;
                case "missing-list": b.U8(0x8a); b.U8(1); break;
                case "short-guid": b.U8(0x81); b.U8(1); break;
                case "short-string": b.U8(0x85); b.Var(100); b.U8(1); break;
                case "overflow-varint": b.U8(0x86); b.Text("key"); b.Raw(Enumerable.Repeat((byte)0x80, 10).ToArray()); break;
            }
        });
        // The bad block is in the first pipe buffer; the tail exceeds the default 64 KiB
        // threshold. If a larger pipe buffers the entire tail, hold the next stream read until
        // cancellation instead: parser failure must release the producer in either scenario.
        fixture.Block(0x7f, b => b.Raw(new byte[256 * 1024]));
        var bytes = fixture.Finish();
        using var stream = new FragmentStream(bytes, 64 * 1024, holdEndUntilCancelled: true);
        var error = await CaptureError(stream);
        bool invalidData = corruption.StartsWith("unknown", StringComparison.Ordinal) || corruption == "overflow-varint";
        Assert.Multiple(() =>
        {
            Assert.That(bytes.Length, Is.GreaterThan(64 * 1024));
            Assert.That(error, invalidData ? Is.TypeOf<InvalidDataException>() : Is.TypeOf<EndOfStreamException>());
            Assert.That(stream.LastReadToken.IsCancellationRequested, Is.True, "producer read token must be cancelled on parser failure");
            Assert.That(stream.ActiveReads, Is.Zero, "no stream read may remain in flight");
            Assert.That(stream.BytesRead, Is.LessThanOrEqualTo(bytes.Length), "read-ahead may buffer the tail but producer must terminate");
        });
    }

    private sealed class InlineProgress(Action<NetTraceReader.Progression> report) : IProgress<NetTraceReader.Progression>
    {
        public void Report(NetTraceReader.Progression value) => report(value);
    }

    private static V6TraceFixture BasicFixture(FieldSpec[]? fields = null)
    {
        var fixture = new V6TraceFixture();
        fixture.Thread(1, OsThread, OsProcess, "worker雪😀");
        fixture.Thread(2, CaptureOsThread, 99, "capture", extraProperty: true);
        fixture.Metadata(1, "Synthetic", fields);
        return fixture;
    }

    private static async Task<Trace> Read(FragmentStream stream)
    {
        var task = new NetTraceReader(stream).ReadFullTraceAsync();
        try { return await task.WaitAsync(ReadTimeout); }
        finally { stream.Abort(); }
    }

    private static async Task<Exception?> ReadError(byte[] bytes, int fragmentSize = 65536)
    {
        using var stream = new FragmentStream(bytes, fragmentSize);
        return await CaptureError(stream);
    }

    private static async Task<Exception?> CaptureError(FragmentStream stream)
    {
        try { await Read(stream); return null; }
        catch (Exception error) { return error; }
    }

}
