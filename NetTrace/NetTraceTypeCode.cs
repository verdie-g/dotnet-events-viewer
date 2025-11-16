namespace NetTrace;

public enum NetTraceTypeCode
{
    /// <summary>Concatenate together all of the encoded fields.</summary>
    Object = 1,

    /// <summary>A 4-byte LE integer with value 0=false and 1=true..</summary>
    Boolean32 = 3,

    /// <summary>a 2-byte UTF16 code unit.</summary>
    Utf16CodeUnit = 4,

    /// <summary>1-byte signed integer.</summary>
    SByte = 5,

    /// <summary>1-byte unsigned integer.</summary>
    Byte = 6,

    /// <summary>2-byte signed LE integer.</summary>
    Int16 = 7,

    /// <summary>2-byte unsigned LE integer.</summary>
    UInt16 = 8,

    /// <summary>4-byte signed LE integer.</summary>
    Int32 = 9,

    /// <summary>4-byte unsigned LE integer.</summary>
    UInt32 = 10,

    /// <summary>8-byte signed LE integer.</summary>
    Int64 = 11,

    /// <summary>8-byte unsigned LE integer.</summary>
    UInt64 = 12,

    /// <summary>4-byte single-precision IEEE754 floating point value.</summary>
    Single = 13,

    /// <summary>8-byte double-precision IEEE754 floating point value.</summary>
    Double = 14,

    /// <summary>16-byte decimal value.</summary>
    Decimal = 15,

    /// <summary>Encoded as 8 concatenated Int16s representing year, month, dayOfWeek, day, hour, minute, second, and milliseconds.</summary>
    DateTime = 16,

    /// <summary>A 16-byte guid encoded as the concatenation of an Int32; 2 Int16s; and 8 Uint8s.</summary>
    Guid = 17,

    /// <summary>A string encoded with UTF16 characters and a 2-byte null terminator.</summary>
    NullTerminatedUtf16String = 18,

    /// <summary>A UInt16 length-prefixed variable-sized array. Elements are encoded depending on the ElementType.</summary>
    Array = 19,

    /// <summary>Variable-length signed integer with zig-zag encoding (same as protobuf).</summary>
    VarInt = 20,

    /// <summary>Variable-length unsigned integer (ULEB128).</summary>
    VarUInt = 21,

    /// <summary>A fixed-length array of elements. The length is determined by the metadata.</summary>
    FixedLengthArray = 22,

    /// <summary>A single UTF8 code unit (1 byte).</summary>
    Utf8CodeUnit = 23,

    /// <summary>An array at a relative location within the payload.</summary>
    RelLoc = 24,

    /// <summary>An absolute data location within the payload.</summary>
    DataLoc = 25,

    /// <summary>A 1 byte boolean with value 0=false and 1=true.</summary>
    Boolean8 = 26,
}
