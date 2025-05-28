namespace NetTrace.FastSerializer;

public enum FastSerializerTag : byte
{
    NullReference = 1,
    BeginPrivateObject = 5,
    EndObject = 6,
}