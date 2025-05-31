using Lagrange.Proto.Primitives;
using Lagrange.Proto.Serialization.Metadata;

namespace Lagrange.Proto.Serialization.Converter;

public class ProtoNullableConverter<T> : ProtoConverter<T?> where T : struct
{
    private readonly ProtoConverter<T> _converter = ProtoTypeResolver.GetConverter<T>();

    public override void Write<TBufferWriter>(int field, WireType wireType, ProtoWriter<TBufferWriter> writer, T? value)
    {
        if (value.HasValue) _converter.Write(0, wireType, writer, value.Value);
    }

    public override int Measure(int field, WireType wireType, T? value)
    {
        return value.HasValue ? _converter.Measure(field, wireType, value.Value) : 0;
    }

    public override T? Read(int field, WireType wireType, ref ProtoReader reader)
    {
        return _converter.Read(field, wireType, ref reader);
    }
}