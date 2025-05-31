using System.Buffers;
using Lagrange.Proto.Primitives;

namespace Lagrange.Proto.Serialization;

public abstract class ProtoConverter
{
}

public abstract class ProtoConverter<T> : ProtoConverter
{
    public virtual bool ShouldSerialize(T value, bool ignoreDefaultValue) => value != null;

    public abstract void Write<TBufferWriter>(int field, WireType wireType, ProtoWriter<TBufferWriter> writer, T value) where TBufferWriter : struct, IBufferWriter<byte>;
    
    public virtual void WriteWithNumberHandling<TBufferWriter>(int field, WireType wireType, ProtoWriter<TBufferWriter> writer, T value, ProtoNumberHandling numberHandling) where TBufferWriter : struct, IBufferWriter<byte> => 
        Write(field, wireType, writer, value);

    public abstract int Measure(int field, WireType wireType, T value);
    
    public abstract T Read(int field, WireType wireType, ref ProtoReader reader);
    
    public virtual T ReadWithNumberHandling(int field, WireType wireType, ref ProtoReader reader, ProtoNumberHandling numberHandling) => Read(field, wireType, ref reader);
}