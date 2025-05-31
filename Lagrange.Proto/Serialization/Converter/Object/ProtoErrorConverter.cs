using Lagrange.Proto.Primitives;

namespace Lagrange.Proto.Serialization.Converter;

public class ProtoErrorConverter<T> : ProtoConverter<T>
{
    public override void Write<TBufferWriter>(int field, WireType wireType, ProtoWriter<TBufferWriter> writer, T value)
    {
        ThrowHelper.ThrowInvalidOperationException_FailedDetermineConverter<T>();
    }

    public override int Measure(int field, WireType wireType, T value)
    {
        ThrowHelper.ThrowInvalidOperationException_FailedDetermineConverter<T>();
        return 0;
    }

    public override T Read(int field, WireType wireType, ref ProtoReader reader)
    {
        ThrowHelper.ThrowInvalidOperationException_FailedDetermineConverter<T>();
        return default!;
    }
}