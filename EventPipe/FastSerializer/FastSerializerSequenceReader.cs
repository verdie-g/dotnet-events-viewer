using System.Buffers;
using System.Buffers.Binary;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Text;

namespace EventPipe.FastSerializer;

internal ref struct FastSerializerSequenceReader(ReadOnlySequence<byte> buffer, long readStartPosition)
{
    private SequenceReader<byte> _reader = new(buffer);

    /// <summary>Opaque position that should only be used to advance in the <see cref="ReadOnlySequence{T}"/>.</summary>
    public SequencePosition Position => _reader.Position;

    /// <summary>Absolute position that should be used to compute a padding or a block end.</summary>
    public long AbsolutePosition => readStartPosition + _reader.Consumed;
    public long Consumed => _reader.Consumed;
    public long Remaining => _reader.Remaining;
    public bool End => _reader.End;
    public ReadOnlySpan<byte> UnreadSpan => _reader.UnreadSpan;

    public void Advance(long count)
    {
        _reader.Advance(count);
    }

    public byte ReadByte()
    {
        ThrowIfFalse(TryReadByte(out byte value));
        return value;
    }

    public bool TryReadByte(out byte value)
    {
        return _reader.TryRead(out value);
    }

    public ReadOnlySequence<byte> ReadBytes(int count)
    {
        ThrowIfFalse(_reader.TryReadExact(count, out ReadOnlySequence<byte> value));
        return value;
    }

    public bool TryReadBytes(int count, out ReadOnlySequence<byte> value)
    {
        return _reader.TryReadExact(count, out value);
    }

    public short ReadInt16()
    {
        ThrowIfFalse(TryReadInt16(out short value));
        return value;
    }

    public bool TryReadInt16(out short value)
    {
        return _reader.TryReadLittleEndian(out value);
    }

    public ushort ReadUInt16()
    {
        return (ushort)ReadInt16();
    }

    public int ReadInt32()
    {
        ThrowIfFalse(TryReadInt32(out int value));
        return value;
    }

    public bool TryReadInt32(out int value)
    {
        return _reader.TryReadLittleEndian(out value);
    }

    public uint ReadUInt32()
    {
        return (uint)ReadInt32();
    }

    public long ReadInt64()
    {
        ThrowIfFalse(TryReadInt64(out long value));
        return value;
    }

    public bool TryReadInt64(out long value)
    {
        return _reader.TryReadLittleEndian(out value);
    }

    public ulong ReadUInt64()
    {
        return (ulong)ReadInt64();
    }

    public float ReadSingle()
    {
        ThrowIfFalse(TryReadSingle(out float value));
        return value;
    }

    public bool TryReadSingle(out float value)
    {
        if (!TryReadBytes(sizeof(float), out var singleSeq))
        {
            value = default;
            return false;
        }

        Span<byte> singleBytes = stackalloc byte[(int)singleSeq.Length];
        singleSeq.CopyTo(singleBytes);
        value = BinaryPrimitives.ReadSingleBigEndian(singleBytes);
        return true;
    }

    public double ReadDouble()
    {
        ThrowIfFalse(TryReadDouble(out double value));
        return value;
    }

    public bool TryReadDouble(out double value)
    {
        if (!TryReadBytes(sizeof(double), out var doubleSeq))
        {
            value = default;
            return false;
        }

        Span<byte> doubleBytes = stackalloc byte[(int)doubleSeq.Length];
        doubleSeq.CopyTo(doubleBytes);
        value = BinaryPrimitives.ReadDoubleBigEndian(doubleBytes);
        return true;
    }

    public bool TryReadString(out ReadOnlySequence<byte> value)
    {
        if (!TryReadInt32(out int length))
        {
            value = default;
            return false;
        }

        return TryReadBytes(length, out value);
    }

    public string ReadNullTerminatedString()
    {
        var unreadCharSpan = MemoryMarshal.Cast<byte, char>(UnreadSpan);
        int nullIdx = unreadCharSpan.IndexOf((char)0);
        if (nullIdx == 0)
        {
            Advance(sizeof(char));
            return "";
        }

        if (nullIdx != -1)
        {
            string str = new(unreadCharSpan[..nullIdx]);
            Advance((nullIdx + 1) * sizeof(char));
            return str;
        }

        // Ain't nobody got time for that.
        return ReadNullTerminatedUtf16StringSlow(ref this);

        static string ReadNullTerminatedUtf16StringSlow(ref FastSerializerSequenceReader reader)
        {
            StringBuilder sb = new();

            while (true)
            {
                short c = reader.ReadInt16();
                if (c == 0)
                {
                    break;
                }

                sb.Append(Convert.ToChar(c));
            }

            return sb.ToString();
        }
    }

    public Guid ReadGuid()
    {
        ThrowIfFalse(TryReadGuid(out Guid value));
        return value;
    }

    public bool TryReadGuid(out Guid value)
    {
        const int guidSize = 16;
        if (!_reader.TryReadExact(guidSize, out var seq))
        {
            value = default;
            return false;
        }

        Span<byte> bytes = stackalloc byte[guidSize];
        seq.CopyTo(bytes);
        value = new Guid(bytes);
        return true;
    }

    public int ReadVarInt32()
    {
        ThrowIfFalse(TryReadVarInt32(out int value));
        return value;
    }

    public bool TryReadVarInt32(out int value)
    {
        uint res = 0;
        byte b;
        const int maxBytesWithoutOverflow = 4;
        for (int i = 0; i < maxBytesWithoutOverflow * 7; i += 7)
        {
            if (!_reader.TryRead(out b))
            {
                value = default;
                return false;
            }

            res |= (b & 0x7Fu) << i;
            if (b <= 0x7F)
            {
                value = (int)res;
                return true;
            }
        }

        if (!_reader.TryRead(out b))
        {
            value = default;
            return false;
        }

        if (b > 0b_1111u)
        {
            throw new FormatException();
        }

        res |= (uint)b << (maxBytesWithoutOverflow * 7);
        value = (int)res;
        return true;
    }

    public long ReadVarInt64()
    {
        ThrowIfFalse(TryReadVarInt64(out long value));
        return value;
    }

    public bool TryReadVarInt64(out long value)
    {
        ulong res = 0;
        byte b;
        const int maxBytesWithoutOverflow = 9;
        for (int i = 0; i < maxBytesWithoutOverflow * 7; i += 7)
        {
            if (!_reader.TryRead(out b))
            {
                value = default;
                return false;
            }

            res |= (b & 0x7Ful) << i;
            if (b <= 0x7F)
            {
                value = (long)res;
                return true;
            }
        }

        if (!_reader.TryRead(out b))
        {
            value = default;
            return false;
        }

        if (b > 0b_1u)
        {
            throw new FormatException();
        }

        res |= (ulong)b << (maxBytesWithoutOverflow * 7);
        value = (long)res;
        return true;
    }

    private void ThrowIfFalse([DoesNotReturnIf(false)] bool b)
    {
        CorruptedBlockException.ThrowIfFalse(b, AbsolutePosition);
    }
}