using System.Text;

namespace NetTrace.Test;

/// <summary>Independent V6 wire-format builder for valid and deliberately malformed synthetic traces.</summary>
internal sealed class V6TraceFixture
{
    public static readonly Guid SampleProviderGuid = new("00112233-4455-6677-8899-aabbccddeeff");
    public static readonly TimeSpan ReadTimeout = TimeSpan.FromSeconds(10);

    public static async Task<Trace> ReadTraceAsync(byte[] bytes, int fragmentSize = 65536)
    {
        using var stream = new FragmentStream(bytes, fragmentSize);
        var reading = new NetTraceReader(stream).ReadFullTraceAsync();
        try { return await reading.WaitAsync(ReadTimeout); }
        finally { stream.Abort(); }
    }

    // Fixture types use literal wire identifiers, never production serializers or MS TraceEvent.
    // Layout reference: PerfView src/TraceEvent/EventPipe/NetTraceFormat.md (V6, LabelListBlock).
    // Deliberate compatibility exception: this parser's scalar type 23 is a UInt16-prefixed UTF8
    // string, NOT the UTF8CodeUnit character used by the newer reference document.
    public sealed record Shape(byte Kind, Shape? Element = null, ushort Count = 0, FieldSpec[]? Fields = null);
    public sealed record FieldSpec(string Name, Shape Type);
    public static FieldSpec Field(string name, byte kind) => Field(name, new Shape(kind));
    public static FieldSpec Field(string name, Shape type) => new(name, type);
    public sealed record LabelSpec(byte Kind, byte[] Value);
    public static LabelSpec Label(byte kind, Action<Wire> value) => new(kind, Bytes(value));
    public sealed record Row(int Metadata = 1, int Sequence = 1, long Thread = 1, long Capture = 2,
        int Processor = 0, int Stack = 0, long Timestamp = 1001, int LabelId = 0, byte[]? Payload = null);

    public static byte[] Bytes(Action<Wire> write)
    {
        using var wire = new Wire();
        write(wire);
        return wire.ToArray();
    }

    public sealed class Wire : IDisposable
    {
        private readonly MemoryStream _stream = new();
        private readonly BinaryWriter _writer;
        public Wire() => _writer = new BinaryWriter(_stream, Encoding.UTF8, leaveOpen: true);
        public byte[] ToArray() => _stream.ToArray();
        public void Raw(byte[] bytes) => _writer.Write(bytes);
        public void U8(byte value) => _writer.Write(value);
        public void I16(short value) => _writer.Write(value);
        public void U16(ushort value) => _writer.Write(value);
        public void I32(int value) => _writer.Write(value);
        public void U32(uint value) => _writer.Write(value);
        public void I64(long value) => _writer.Write(value);
        public void U64(ulong value) => _writer.Write(value);
        // Size prefixes exclude themselves in V6 metadata, fields, threads and optional data.
        // A declared-size override lets malformed fixtures retain neighboring bytes intact.
        public void Sized16(byte[] body, ushort? declaredSize = null)
        {
            U16(declaredSize ?? checked((ushort)body.Length));
            Raw(body);
        }
        public void Pointer(ulong value, int pointerSize)
        {
            if (pointerSize == 4) U32(checked((uint)value));
            else if (pointerSize == 8) U64(value);
            else throw new ArgumentOutOfRangeException(nameof(pointerSize));
        }
        public void Guid(Guid value) => Raw(value.ToByteArray());
        public void Var(ulong value)
        {
            while (value >= 0x80) { U8((byte)((value & 0x7f) | 0x80)); value >>= 7; }
            U8((byte)value);
        }
        public void ZigZag(long value) => Var(unchecked(((ulong)value << 1) ^ (ulong)(value >> 63)));
        public void Text(string value)
        {
            var bytes = Encoding.UTF8.GetBytes(value);
            Var((ulong)bytes.Length); Raw(bytes);
        }
        public void ShortText(string value)
        {
            var bytes = Encoding.UTF8.GetBytes(value);
            U16(checked((ushort)bytes.Length)); Raw(bytes);
        }
        public void Utf16(string value) { Raw(Encoding.Unicode.GetBytes(value)); U16(0); }
        public void Dispose() { _writer.Dispose(); _stream.Dispose(); }
    }

    private readonly List<byte> _bytes = [];
    private readonly int _pointerSize;
    public V6TraceFixture(int pointerSize = 8)
    {
        _pointerSize = pointerSize;
        _bytes.AddRange(Bytes(b => { b.Raw("Nettrace"u8.ToArray()); b.I32(0); b.I32(6); b.I32(0); }));
        Block(1, b =>
        {
            foreach (short value in new short[] { 2026, 1, 5, 2, 3, 4, 5, 6 }) b.I16(value);
            b.I64(1000); b.I64(1_000_000_000); b.I32(pointerSize); b.I32(4);
            b.Text("ProcessId"); b.Text("42"); b.Text("HardwareThreadCount"); b.Text("16");
            b.Text("ExpectedCPUSamplingRate"); b.Text("1000"); b.Text("unknown"); b.Text("ignored雪");
        });
    }
    public void Block(byte kind, Action<Wire> write) => Block(kind, Bytes(write));

    public void Block(byte kind, byte[] body)
    {
        if (body.Length > 0xffffff) throw new ArgumentOutOfRangeException(nameof(body));
        _bytes.AddRange(Bytes(b => { b.U32(((uint)kind << 24) | (uint)body.Length); b.Raw(body); }));
    }
    public byte[] Finish() => _bytes.Concat(new byte[4]).ToArray();
    public void Thread(ulong index, long threadId, long? processId, string? name, bool extraProperty = false)
    {
        Block(6, b =>
        {
            var row = Bytes(r =>
            {
                r.Var(index);
                if (name != null) { r.U8(1); r.Text(name); }
                if (processId.HasValue) { r.U8(2); r.Var(checked((ulong)processId.Value)); }
                r.U8(3); r.Var(checked((ulong)threadId));
                if (extraProperty) { r.U8(4); r.Text("custom雪"); r.Text("ignored"); }
            });
            b.Sized16(row);
        });
    }
    public void Metadata(int id, string name, FieldSpec[]? fields = null, string provider = "Synthetic.Provider",
        int eventId = 123, byte version = 3, bool extended = false)
    {
        Block(3, b =>
        {
            b.U16((ushort)(extended ? 3 : 0));
            if (extended) b.Raw([0xde, 0xad, 0xbe]);
            var row = Bytes(r =>
            {
                r.Var((ulong)id); r.Text(provider); r.Var((ulong)eventId); r.Text(name);
                WriteFields(r, fields ?? [], extended);
                var optional = Bytes(o =>
                {
                    o.U8(1); o.U8(1); o.U8(3); o.U64(0x1234); o.U8(8); o.U8(4); o.U8(9); o.U8(version);
                    if (extended)
                    {
                        o.U8(4); o.Text("value={値}"); o.U8(5); o.Text("description雪");
                        o.U8(6); o.Text("key"); o.Text("value"); o.U8(7); o.Guid(SampleProviderGuid);
                    }
                });
                r.Sized16(optional);
                if (extended) r.Raw([0xfa, 0xce]);
            });
            b.Sized16(row);
        });
    }
    private static void WriteFields(Wire b, FieldSpec[] fields, bool extended = false)
    {
        b.U16(checked((ushort)fields.Length));
        foreach (var field in fields)
        {
            var body = Bytes(f => { f.Text(field.Name); WriteType(f, field.Type); if (extended) f.Raw([0xaa, 0xbb]); });
            b.Sized16(body);
        }
    }
    private static void WriteType(Wire b, Shape type)
    {
        b.U8(type.Kind);
        if (type.Kind is 19 or 22 or 24 or 25)
        {
            WriteType(b, type.Element!);
            if (type.Kind == 22) b.U16(type.Count);
        }
        else if (type.Kind == 1) WriteFields(b, type.Fields ?? []);
    }
    public void Labels(int firstId, params LabelSpec[][] lists)
    {
        Block(8, b =>
        {
            b.I32(firstId); b.I32(lists.Length);
            foreach (var labels in lists)
            {
                if (labels.Length == 0) throw new ArgumentException("Empty lists are implicit id 0");
                for (int i = 0; i < labels.Length; i++)
                {
                    b.U8((byte)(labels[i].Kind | (i == labels.Length - 1 ? 0x80 : 0)));
                    b.Raw(labels[i].Value);
                }
            }
        });
    }
    public void Stack(int id, params ulong[] addresses) => Block(5, b =>
    {
        b.I32(id); b.I32(1); b.I32(addresses.Length * _pointerSize);
        foreach (ulong address in addresses) b.Pointer(address, _pointerSize);
    });
    public void SequencePoint(int flags) => Block(4, b =>
    {
        b.I64(1001); b.I32(flags); b.I32(2); b.Var(1); b.Var(1); b.Var(2); b.Var(1);
    });
    public static void WriteUncompressedRow(Wire b, Row row, int? eventSize = null, int? payloadSize = null)
    {
        var payload = row.Payload ?? [];
        b.I32(eventSize ?? (48 + payload.Length)); b.U32((uint)row.Metadata | 0x80000000u);
        b.I32(row.Sequence); b.I64(row.Thread); b.I64(row.Capture); b.I32(row.Processor);
        b.I32(row.Stack); b.I64(row.Timestamp); b.I32(row.LabelId); b.I32(payloadSize ?? payload.Length);
        b.Raw(payload);
    }
    public void Events(bool compressed, params Row[] rows) => Block(2, EventBlockBody(compressed, rows));

    // Expose the complete body so boundary tests can truncate it without corrupting sibling blocks.
    public static byte[] EventBlockBody(bool compressed, params Row[] rows)
    {
        return Bytes(b =>
        {
            b.U16(20); b.U16((ushort)(compressed ? 1 : 0));
            b.I64(rows.Min(r => r.Timestamp)); b.I64(rows.Max(r => r.Timestamp));
            var previous = new Row(Metadata: 0, Sequence: 0, Thread: 0, Capture: 0, Timestamp: 0);
            foreach (var row in rows)
            {
                var payload = row.Payload ?? [];
                if (!compressed)
                {
                    WriteUncompressedRow(b, row);
                }
                else
                {
                    bool group = row.Sequence != previous.Sequence + 1 || row.Capture != previous.Capture || row.Processor != previous.Processor;
                    byte flags = 0x40; // IsSorted; bit 0x20 has no field in V6.
                    if (row.Metadata != previous.Metadata) flags |= 1;
                    if (group) flags |= 2;
                    if (row.Thread != previous.Thread) flags |= 4;
                    if (row.Stack != previous.Stack) flags |= 8;
                    if (row.LabelId != previous.LabelId) flags |= 0x10;
                    if (payload.Length != (previous.Payload?.Length ?? 0)) flags |= 0x80;
                    b.U8(flags);
                    if ((flags & 1) != 0) b.Var(checked((ulong)row.Metadata));
                    if (group)
                    {
                        b.Var(checked((ulong)(row.Sequence - previous.Sequence - 1)));
                        b.Var(checked((ulong)row.Capture)); b.Var(checked((ulong)row.Processor));
                    }
                    if ((flags & 4) != 0) b.Var(checked((ulong)row.Thread));
                    if ((flags & 8) != 0) b.Var(checked((ulong)row.Stack));
                    b.Var(checked((ulong)(row.Timestamp - previous.Timestamp)));
                    if ((flags & 0x10) != 0) b.Var(checked((ulong)row.LabelId));
                    if ((flags & 0x80) != 0) b.Var((ulong)payload.Length);
                }
                if (compressed) b.Raw(payload);
                previous = row;
            }
        });
    }

    // Yield on every read so one-byte tests exercise incremental PipeReader input rather than
    // MemoryStream's synchronous fast path. The abort token bounds producer reads even on timeout.
    public sealed class FragmentStream(byte[] bytes, int fragmentSize, bool holdEndUntilCancelled = false,
        int cancelAtBoundary = -1) : Stream
    {
        private readonly CancellationTokenSource _abort = new();
        private int _position;
        private int _activeReads;
        public TaskCompletionSource BoundaryReached { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public bool BoundaryReadCancelled { get; private set; }
        public int BytesRead => _position;
        public int ActiveReads => Volatile.Read(ref _activeReads);
        public CancellationToken LastReadToken { get; private set; }
        public void Abort() => _abort.Cancel();
        public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            LastReadToken = cancellationToken;
            Interlocked.Increment(ref _activeReads);
            try
            {
                await Task.Yield();
                cancellationToken.ThrowIfCancellationRequested(); _abort.Token.ThrowIfCancellationRequested();
                if (_position == cancelAtBoundary || (_position == bytes.Length && holdEndUntilCancelled))
                {
                    using var linked = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _abort.Token);
                    Task blockedRead = Task.Delay(Timeout.InfiniteTimeSpan, linked.Token);
                    if (_position == cancelAtBoundary) BoundaryReached.TrySetResult();
                    try { await blockedRead; }
                    catch (OperationCanceledException) when (_position == cancelAtBoundary && cancellationToken.IsCancellationRequested)
                    {
                        BoundaryReadCancelled = true;
                        throw;
                    }
                }
                int count = Math.Min(Math.Min(buffer.Length, fragmentSize), bytes.Length - _position);
                if (cancelAtBoundary >= 0 && _position < cancelAtBoundary)
                    count = Math.Min(count, cancelAtBoundary - _position);
                bytes.AsMemory(_position, count).CopyTo(buffer); _position += count;
                return count;
            }
            finally { Interlocked.Decrement(ref _activeReads); }
        }
        public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();
        public override bool CanRead => true;
        public override bool CanSeek => false;
        public override bool CanWrite => false;
        public override long Length => bytes.Length;
        public override long Position { get => _position; set => throw new NotSupportedException(); }
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
