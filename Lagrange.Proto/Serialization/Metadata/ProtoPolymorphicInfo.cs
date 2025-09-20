namespace Lagrange.Proto.Serialization.Metadata;


public interface IProtoPolymorphicInfoBase
{
    public uint PolymorphicIndicateIndex { get; set; }
    public bool PolymorphicFallbackToBaseType { get; set; }

    public Type? GetTypeFromDiscriminator(object discriminator);

    public bool SetTypeDiscriminator(object discriminator, Type type);
}


public class ProtoPolymorphicObjectInfo<TKey> : IProtoPolymorphicInfoBase where TKey : IEquatable<TKey>
{
    public uint PolymorphicIndicateIndex { get; set; } = 0;
    public bool PolymorphicFallbackToBaseType { get; set; } = true;

    public Type? GetTypeFromDiscriminator(object discriminator)
    {
        return PolymorphicDerivedTypes.GetValueOrDefault((TKey)discriminator);
    }

    public bool SetTypeDiscriminator(object discriminator, Type type)
    {
        PolymorphicDerivedTypes[(TKey)discriminator] = type;
        return true;
    }

    public Dictionary<TKey, Type> PolymorphicDerivedTypes { get; init; } = [];
}

public class ProtoPolymorphicDerivedTypeDescriptor<TBase>
{
    public Dictionary<uint, ProtoFieldInfo> Fields { get; init; } = [];
    public Func<TBase> ObjectCreator { get; init; } = null!;
    public bool IgnoreDefaultFields { get; init; }
}