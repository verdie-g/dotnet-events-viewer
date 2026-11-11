using System.Buffers.Binary;
using System.Text;

namespace NetTrace.Test;

[TestFixture]
public class NetTraceReaderV5Test
{
    private static readonly TimeSpan ReadTimeout = TimeSpan.FromSeconds(10);
    private static readonly byte[] Header = Convert.FromBase64String("TmV0dHJhY2UUAAAAIUZhc3RTZXJpYWxpemF0aW9uLjE=");
    // The compact MetadataBlock + two compressed TaskWaitBegin events from NetTraceReaderTest.
    // Keep it separate from TraceObjectTest: inserting that object changes absolute block padding.
    private static readonly byte[] Objects = Convert.FromBase64String("BQUBAgAAAAIAAAANAAAATWV0YWRhdGFCbG9jawZqAQAAAAAAFAABACtLjC4YzQUAK0uMLhjNBQDG/////w8A/////w+OFKuWsfSCo/MCvgIBAAAAUwB5AHMAdABlAG0ALgBUAGgAcgBlAGEAZABpAG4AZwAuAFQAYQBzAGsAcwAuAFQAcABsAEUAdgBlAG4AdABTAG8AdQByAGMAZQAAAAoAAABUAGEAcwBrAFcAYQBpAHQAQgBlAGcAaQBuAAAAAwAAAADwAAADAAAABAAAAAUAAAAJAAAATwByAGkAZwBpAG4AYQB0AGkAbgBnAFQAYQBzAGsAUwBjAGgAZQBkAHUAbABlAHIASQBEAAAACQAAAE8AcgBpAGcAaQBuAGEAdABpAG4AZwBUAGEAcwBrAEkARAAAAAkAAABUAGEAcwBrAEkARAAAAAkAAABCAGUAaABhAHYAaQBvAHIAAAAJAAAAQwBvAG4AdABpAG4AdQBlAFcAaQB0AGgAVABhAHMAawBJAEQAAAABAAAAAQkGBQUBAgAAAAIAAAAKAAAARXZlbnRCbG9jawZXAAAAAAAAFAABACtLjC4YzQUAboSOLhjNBQDPAQCCFP////8PghQBq5ax9IKj8wIUAQAAAAAAAAAEAAAAAgAAAAUAAAAIAsPyCAEAAAAAAAAABQAAAAIAAAADAAAABg==");
    private const int MetadataObjectLength = 399;
    private const int CompressedEventBodyOffset = 432;

    [TestCase(true, false)]
    [TestCase(true, true)]
    [TestCase(false, false)]
    [TestCase(false, true)]
    public async Task SmallAsyncFragments_DoNotReplayEvents(bool compressed, bool variableChunks)
    {
        byte[] bytes = Fixture(compressed);
        var baseline = await ReadBaseline(bytes);
        using var stream = new FragmentStream(bytes, variableChunks ? [1, 7, 2, 3, 11, 1, 5] : [1]);
        var actual = await Read(stream);
        AssertEquivalent(baseline, actual);
        Assert.That(stream.ReadCalls, Is.GreaterThan(20));
    }

    [TestCase(true, false)]
    [TestCase(true, true)]
    [TestCase(false, false)]
    [TestCase(false, true)]
    public async Task EventBodyBeforeEndObject_IsCommittedOnlyOnce(bool compressed, bool largeTail)
    {
        byte[] bytes = Fixture(compressed);
        int endObjectOffset = bytes.Length - 2;
        Assert.That(bytes[endObjectOffset], Is.EqualTo((byte)6), "outer FastSerializer EndObject");
        var baseline = await ReadBaseline(bytes);
        // Hold EndObject back after the entire event body has been flushed to the pipe.
        // This is the dangerous boundary: the old reader added both events, returned false
        // for the missing tag, then replayed the same object on the next PipeReader buffer.
        using var stream = new FragmentStream(bytes, [65536], endObjectOffset,
            largeTail ? 64 * 1024 * 1024 : 0);
        var metadataRead = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var reader = new NetTraceReader(stream).AddProgress(new InlineProgress(p =>
        {
            if (p.BytesRead == Header.Length + MetadataObjectLength) metadataRead.TrySetResult();
        }));
        Task<Trace> reading = reader.ReadFullTraceAsync();
        try
        {
            await stream.BoundaryReached.Task.WaitAsync(ReadTimeout);
            await metadataRead.Task.WaitAsync(ReadTimeout);
            // Allow the consumer to finish examining the already-flushed body while the
            // producer is explicitly gated. Task.Yield alone can coalesce both fragments.
            await Task.Delay(50);
            Assert.That(reading.IsCompleted, Is.False, "EndObject and the null footer are still withheld");
            stream.ReleaseBoundary();
            var actual = await reading.WaitAsync(ReadTimeout);
            AssertEquivalent(baseline, actual);
            Assert.Multiple(() =>
            {
                Assert.That(stream.LastReadToken.IsCancellationRequested, Is.True);
                Assert.That(stream.ActiveReads, Is.Zero);
                if (largeTail)
                    Assert.That(stream.BytesRead, Is.LessThan(stream.Length),
                        "stop at NullReference without draining a tail larger than pipe backpressure");
            });
        }
        finally
        {
            stream.Abort();
            stream.ReleaseBoundary();
        }
    }

    [Test]
    public async Task UncompressedV5Fixture_MatchesOriginalTwoEventSample()
    {
        AssertEquivalent(await ReadBaseline(Fixture(true)), await ReadBaseline(Fixture(false)));
    }

    [TestCase(false, "row-header")]
    [TestCase(true, "row-header")]
    [TestCase(false, "payload")]
    [TestCase(true, "payload")]
    [TestCase(false, "block-timestamp")]
    [TestCase(true, "block-timestamp")]
    [TestCase(false, "block-extension")]
    [TestCase(true, "block-extension")]
    public async Task EventBlockBoundaries_RejectRowsAndHeadersThatWouldConsumeSiblingObject(bool compressed, string corruption)
    {
        byte[] complete = Fixture(compressed);
        int bodyOffset = Header.Length + CompressedEventBodyOffset;
        // Locate the size from the fixed EventBlock SerializationType, not absolute padding.
        int sizeOffset = Header.Length + MetadataObjectLength + 26;
        int bodySize = BinaryPrimitives.ReadInt32LittleEndian(complete.AsSpan(sizeOffset, 4));
        byte[] body = complete.AsSpan(bodyOffset, bodySize).ToArray();
        int declaredSize = corruption switch
        {
            "row-header" => 21, // first row has only its first header byte
            "payload" => bodySize - 1, // second row's final payload byte is outside the block
            "block-timestamp" => 19, // maximum QPC timestamp is incomplete
            "block-extension" => 24,
            _ => throw new ArgumentException(corruption)
        };
        if (corruption == "block-extension")
            BinaryPrimitives.WriteUInt16LittleEndian(body, 28);
        using var trace = new MemoryStream();
        using var writer = new BinaryWriter(trace, Encoding.UTF8, leaveOpen: true);
        writer.Write(Header);
        writer.Write(Objects.AsSpan(0, MetadataObjectLength));
        WriteBlock(writer, trace, "EventBlock", body.AsSpan(0, declaredSize));
        // This is a complete, well-formed sibling FastSerializer object, not truncated input.
        // The old unbounded decoder could borrow its EndObject/type/padding/body bytes.
        WriteBlock(writer, trace, "FutureBlock", new byte[256]);
        writer.Write((byte)1);
        using var stream = new FragmentStream(trace.ToArray(), [65536]);
        await Assert.ThrowsAsync<EndOfStreamException>(async () => await Read(stream),
            "required event bytes must end at the declared block, before the sibling object");
    }

    private static void WriteBlock(BinaryWriter writer, MemoryStream trace, string name, ReadOnlySpan<byte> body)
    {
        byte[] typeName = Encoding.UTF8.GetBytes(name);
        writer.Write(new byte[] { 5, 5, 1 });
        writer.Write(2); writer.Write(2); writer.Write(typeName.Length); writer.Write(typeName);
        writer.Write((byte)6); // end SerializationType
        writer.Write(body.Length);
        while (trace.Position % 4 != 0) writer.Write((byte)0);
        writer.Write(body);
        writer.Write((byte)6); // end outer object
    }

    private static void AssertEquivalent(Trace expected, Trace actual)
    {
        Assert.Multiple(() =>
        {
            Assert.That(expected.Events, Has.Count.EqualTo(2));
            Assert.That(actual.Events, Has.Count.EqualTo(2), "a fragmented object must not replay its events");
            Assert.That(actual.Events.Select(e => e.Index), Is.EqualTo(new[] { 0, 1 }));
            Assert.That(actual.Events.Select(e => e.SequenceNumber), Is.EqualTo(new[] { 1, 2 }));
            Assert.That(actual.Events.Select(e => e.Payload["TaskID"]), Is.EqualTo(new[] { 4, 5 }));
            Assert.That(actual.EventMetadata, Has.Count.EqualTo(1));
            Assert.That(actual.Events[1].Metadata, Is.SameAs(actual.Events[0].Metadata));
        });
        for (int i = 0; i < 2; i++)
        {
            var a = actual.Events[i];
            var e = expected.Events[i];
            Assert.Multiple(() =>
            {
                Assert.That(a.Index, Is.EqualTo(e.Index));
                Assert.That(a.SequenceNumber, Is.EqualTo(e.SequenceNumber));
                Assert.That(a.ThreadId, Is.EqualTo(e.ThreadId));
                Assert.That(a.CaptureThreadId, Is.EqualTo(e.CaptureThreadId));
                Assert.That(a.StackIndex, Is.EqualTo(e.StackIndex));
                Assert.That(a.TimeStamp, Is.EqualTo(e.TimeStamp));
                Assert.That(a.ActivityId, Is.EqualTo(e.ActivityId));
                Assert.That(a.RelatedActivityId, Is.EqualTo(e.RelatedActivityId));
                Assert.That(a.Metadata.ProviderName, Is.EqualTo(e.Metadata.ProviderName));
                Assert.That(a.Metadata.EventId, Is.EqualTo(e.Metadata.EventId));
                Assert.That(a.Metadata.EventName, Is.EqualTo("TaskWaitBegin"));
                Assert.That(a.Metadata.FieldDefinitions, Is.EqualTo(e.Metadata.FieldDefinitions));
                Assert.That(a.Payload, Is.EquivalentTo(e.Payload));
            });
        }
    }

    private static byte[] Fixture(bool compressed)
    {
        if (compressed) return Header.Concat(Objects).Concat(new byte[] { 1 }).ToArray();
        using var trace = new MemoryStream();
        using var writer = new BinaryWriter(trace, Encoding.UTF8, leaveOpen: true);
        writer.Write(Header);
        writer.Write(Objects.AsSpan(0, MetadataObjectLength));
        using var body = new MemoryStream();
        using (var events = new BinaryWriter(body, Encoding.UTF8, leaveOpen: true))
        {
            // V5 EventBlock header: UInt16 size, UInt16 flags, min/max QPC timestamps.
            events.Write(Objects.AsSpan(CompressedEventBodyOffset, 20));
            body.Position = 2;
            events.Write((ushort)0); // uncompressed
            body.Position = body.Length;
            for (int i = 0; i < 2; i++)
            {
                events.Write(76 + 20); // EventSize excludes its own Int32, includes the V5 header and payload.
                events.Write(1); // MetadataId
                events.Write(i + 1); // sequence
                events.Write(2562L); // thread
                events.Write(2562L); // capture thread
                events.Write(-1); // processor
                events.Write(i + 1); // stack
                events.Write(BinaryPrimitives.ReadInt64LittleEndian(
                    Objects.AsSpan(CompressedEventBodyOffset + 4 + 8 * i, 8)));
                events.Write(Guid.Empty.ToByteArray());
                events.Write(Guid.Empty.ToByteArray());
                events.Write(20); // payload size
                foreach (int value in new[] { 1, 0, 4 + i, 2, i == 0 ? 5 : 3 }) events.Write(value);
            }
        }
        WriteBlock(writer, trace, "EventBlock", body.ToArray());
        writer.Write((byte)1); // NullReference footer
        return trace.ToArray();
    }

    private static async Task<Trace> ReadBaseline(byte[] bytes)
    {
        using var stream = new MemoryStream(bytes);
        return await new NetTraceReader(stream).ReadFullTraceAsync().WaitAsync(ReadTimeout);
    }

    private static async Task<Trace> Read(FragmentStream stream)
    {
        try { return await new NetTraceReader(stream).ReadFullTraceAsync().WaitAsync(ReadTimeout); }
        finally { stream.Abort(); }
    }

    private sealed class InlineProgress(Action<NetTraceReader.Progression> report) : IProgress<NetTraceReader.Progression>
    {
        public void Report(NetTraceReader.Progression value) => report(value);
    }

    private sealed class FragmentStream(byte[] bytes, int[] chunks, int boundary = -1, int tailLength = 0) : Stream
    {
        private readonly CancellationTokenSource _abort = new();
        private readonly TaskCompletionSource _release = new(TaskCreationOptions.RunContinuationsAsynchronously);
        private int _position;
        private int _reads;
        private int _activeReads;
        public TaskCompletionSource BoundaryReached { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public int ReadCalls => _reads;
        public int BytesRead => _position;
        public int ActiveReads => Volatile.Read(ref _activeReads);
        public CancellationToken LastReadToken { get; private set; }
        public void ReleaseBoundary() => _release.TrySetResult();
        public void Abort() => _abort.Cancel();
        public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            LastReadToken = cancellationToken;
            Interlocked.Increment(ref _activeReads);
            try
            {
                await Task.Yield();
                using var linked = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _abort.Token);
                linked.Token.ThrowIfCancellationRequested();
                if (_position == boundary)
                {
                    BoundaryReached.TrySetResult();
                    await _release.Task.WaitAsync(linked.Token);
                }
                int chunk = _position < bytes.Length ? chunks[_reads % chunks.Length] : 65536;
                _reads++;
                int count = Math.Min(Math.Min(buffer.Length, chunk), (int)Length - _position);
                if (boundary >= 0 && _position < boundary) count = Math.Min(count, boundary - _position);
                int dataCount = Math.Min(count, Math.Max(0, bytes.Length - _position));
                bytes.AsMemory(Math.Min(_position, bytes.Length), dataCount).CopyTo(buffer);
                buffer.Span.Slice(dataCount, count - dataCount).Fill(0xff); // invalid data after the footer
                _position += count;
                return count;
            }
            finally { Interlocked.Decrement(ref _activeReads); }
        }
        public override bool CanRead => true;
        public override bool CanSeek => false;
        public override bool CanWrite => false;
        public override long Length => bytes.Length + tailLength;
        public override long Position { get => _position; set => throw new NotSupportedException(); }
        public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();
        public override void Flush() => throw new NotSupportedException();
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();
        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
        protected override void Dispose(bool disposing)
        {
            if (disposing) { Abort(); _abort.Dispose(); }
            base.Dispose(disposing);
        }
    }
}
