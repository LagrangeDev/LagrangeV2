using Lagrange.Proto.Primitives;
using Lagrange.Proto.Serialization.Metadata;

namespace Lagrange.Proto;

public interface IProtoSerializable<T>
{
    public static abstract ProtoObjectInfo<T> TypeInfo { get; }
    
    public static abstract void SerializeHandler(T obj, ProtoWriter writer);
    
    public static abstract int MeasureHandler(T obj);
    
    public static abstract ProtoPolymorphicDerivedTypeDescriptor<T>? GetPolymorphicTypeDescriptor<TKey>(TKey discriminator);
}