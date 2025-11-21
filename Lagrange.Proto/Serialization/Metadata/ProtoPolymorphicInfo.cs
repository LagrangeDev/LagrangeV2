using Lagrange.Proto.Primitives;
using Lagrange.Proto.Utility;

namespace Lagrange.Proto.Serialization.Metadata;


public class ProtoPolymorphicInfoBase
{
    public uint PolymorphicIndicateIndex { get; set; }
    public bool PolymorphicFallbackToBaseType { get; set; }
    public virtual ProtoPolymorphicDerivedTypeDescriptor? GetDerivedTypeDescriptorFromReader(ref ProtoReader reader) => null;
    public virtual void SetDerivedTypeDescriptor(object key, ProtoPolymorphicDerivedTypeDescriptor descriptor) => throw new MissingMethodException();
    public Func<ProtoPolymorphicDerivedTypeDescriptor>? RootTypeDescriptorGetter { get; set; }
    public virtual IEnumerable<ProtoPolymorphicDerivedTypeDescriptor> GetAllDerivedTypeDescriptors() => [];
}


public class ProtoPolymorphicObjectInfo<TKey> : ProtoPolymorphicInfoBase where TKey : IEquatable<TKey>
{
    public Dictionary<TKey, ProtoPolymorphicDerivedTypeDescriptor> PolymorphicDerivedTypes { get; init; } = [];
    public override ProtoPolymorphicDerivedTypeDescriptor? GetDerivedTypeDescriptorFromReader(ref ProtoReader reader)
    {
        uint tag = reader.DecodeVarIntUnsafe<uint>();
        int field = (int)(tag >> 3);
        var wireType = (WireType)(tag & 0x7);
        if (field != PolymorphicIndicateIndex)
        {
            reader.Rewind(-ProtoHelper.GetVarIntLength(tag));
            return null;
        }
        var converter = ProtoTypeResolver.GetConverter<TKey>();
        var key = converter.Read(field, wireType, ref reader);
        var rst = PolymorphicDerivedTypes.GetValueOrDefault(key);
        if (rst == null && !PolymorphicFallbackToBaseType)
            ThrowHelper.ThrowInvalidOperationException_UnknownPolymorphicType(typeof(TKey), key);
        return rst;
    }

    public override void SetDerivedTypeDescriptor(object o, ProtoPolymorphicDerivedTypeDescriptor descriptor)
    {
        if (o is TKey key)
            PolymorphicDerivedTypes[key] = descriptor;
        else
            ThrowHelper.ThrowInvalidOperationException_UnknownPolymorphicType(typeof(TKey), o);
    }

    public override IEnumerable<ProtoPolymorphicDerivedTypeDescriptor> GetAllDerivedTypeDescriptors()
    {
        return PolymorphicDerivedTypes.Values;
    }
}


public class ProtoPolymorphicDerivedTypeDescriptor
{
    public required Type CurrentType { get; init; }
    public Func<Dictionary<uint, ProtoFieldInfo>> FieldsGetter { get; init; } = () => throw new MissingMethodException();
    public Func<bool> IgnoreDefaultFieldsGetter { get; init; } = () => false;
    public Func<ProtoPolymorphicInfoBase?> PolymorphicInfoGetter { get; init; } = () => null;

    public virtual object? CreateObject()
    {
        return null;
    }
}

public class ProtoPolymorphicDerivedTypeDescriptor<TBase> : ProtoPolymorphicDerivedTypeDescriptor
{
    public Func<TBase> ObjectCreator { get; init; } = null!;

    public override object? CreateObject()
    {
        return ObjectCreator();
    }
}