using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Lagrange.Proto.Primitives;
using Lagrange.Proto.Serialization.Converter;
using Lagrange.Proto.Serialization.Metadata;

namespace Lagrange.Proto.Serialization;

public static partial class ProtoSerializer
{
    /// <summary>
    /// Deserialize the ProtoPackable Object from the source buffer, AOT Friendly, annotate the type with <see cref="ProtoPackableAttribute"/> to enable the source generator
    /// </summary>
    /// <param name="data">The source buffer to read from</param>
    /// <typeparam name="T">The type of the object to deserialize</typeparam>
    /// <returns>The deserialized object</returns>
    public static T DeserializeProtoPackable<T>(ReadOnlySpan<byte> data) where T : IProtoSerializable<T>
    {
        var reader = new ProtoReader(data);
        return DeserializeProtoPackableCore<T>(ref reader);
    }

    private static T DeserializeProtoPackableCore<T>(ref ProtoReader reader) where T : IProtoSerializable<T>
    {
        var objectInfo = T.TypeInfo;
        Debug.Assert(objectInfo.ObjectCreator != null);

        T target = objectInfo.ObjectCreator();
        var fields = objectInfo.Fields;
        var polymorphicInfo = objectInfo.PolymorphicInfo;

        if (polymorphicInfo?.PolymorphicIndicateIndex is > 0)
        {
            var root = polymorphicInfo.RootTypeDescriptorGetter?.Invoke();
            if (root is not null)
            {
                fields = root.FieldsGetter();
                polymorphicInfo = root.PolymorphicInfoGetter();
            }
        }
        
        polyDeserialize:
        if (polymorphicInfo?.PolymorphicIndicateIndex is > 0)
        {
            var typeDescriptor = polymorphicInfo.GetDerivedTypeDescriptorFromReader(ref reader);
            if (typeDescriptor is not null)
            {
                fields = typeDescriptor.FieldsGetter();
                polymorphicInfo = typeDescriptor.PolymorphicInfoGetter();
                if (!typeof(T).IsAssignableTo(typeDescriptor.CurrentType))
                {
                    if (typeDescriptor.CreateObject() is T newObj)
                    {
                        target = newObj;
                    }
                    else
                    {
                        ThrowHelper.ThrowInvalidOperationException_CanNotCreateObject(typeDescriptor.CurrentType);
                    }
                }

                goto polyDeserialize;
            }
        }
        
        
        while (!reader.IsCompleted)
        {
            uint tag = reader.DecodeVarIntUnsafe<uint>();
            if (fields.TryGetValue(tag, out var fieldInfo))
            {
                fieldInfo.Read(ref reader, target);
            }
            else
            {
                reader.SkipField((WireType)(tag & 0x07));
            }
        }

        return target;
    }

    /// <summary>
    /// Deserialize the ProtoPackable Object from the source buffer, based on reflection
    /// </summary>
    /// <param name="data">The source buffer to read from</param>
    /// <typeparam name="T">The type of the object to deserialize</typeparam>
    /// <returns>The deserialized object</returns>
    [RequiresUnreferencedCode(SerializationUnreferencedCodeMessage)]
    [RequiresDynamicCode(SerializationRequiresDynamicCodeMessage)]
    public static T Deserialize<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T>(
        ReadOnlySpan<byte> data)
    {
        var reader = new ProtoReader(data);
        return DeserializeCore<T>(ref reader);
    }

    [RequiresUnreferencedCode(SerializationUnreferencedCodeMessage)]
    [RequiresDynamicCode(SerializationRequiresDynamicCodeMessage)]
    private static T DeserializeCore<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T>(
        ref ProtoReader reader)
    {
        var converter = GetConverterOf<T>();
        Debug.Assert(converter.ObjectInfo.ObjectCreator != null);

        T target = converter.ObjectInfo.ObjectCreator();
        var boxed = (object?)target; // avoid multiple times of boxing
        if (boxed is null) ThrowHelper.ThrowInvalidOperationException_CanNotCreateObject(typeof(T));
        var fieldInfos = converter.ObjectInfo.Fields;
        var polymorphicInfo = converter.ObjectInfo.PolymorphicInfo; 
        
        if (polymorphicInfo?.PolymorphicIndicateIndex is > 0)
        {
            var root = polymorphicInfo.RootTypeDescriptorGetter?.Invoke();
            if (root is not null)
            {
                fieldInfos = root.FieldsGetter();
                polymorphicInfo = root.PolymorphicInfoGetter();
            }
        }
        
        startDeserialize:
        if (polymorphicInfo?.PolymorphicIndicateIndex is > 0)
        {
            var polymorphicDescriptor = polymorphicInfo.GetDerivedTypeDescriptorFromReader(ref reader);
            if (polymorphicDescriptor is not null)
            {
                fieldInfos = polymorphicDescriptor.FieldsGetter();
                polymorphicInfo = polymorphicDescriptor.PolymorphicInfoGetter();
                
                if (!typeof(T).IsAssignableTo(polymorphicDescriptor.CurrentType))
                {
                    boxed = polymorphicDescriptor.CreateObject();
                    target = (T)boxed!;
                    if (boxed is null) ThrowHelper.ThrowInvalidOperationException_CanNotCreateObject(polymorphicDescriptor.CurrentType);
                }
                goto startDeserialize;
            }
        }


        while (!reader.IsCompleted)
        {
            uint tag = reader.DecodeVarIntUnsafe<uint>();
            if (fieldInfos.TryGetValue(tag, out var fieldInfo))
            {
                fieldInfo.Read(ref reader, boxed);
            }
            else
            {
                reader.SkipField((WireType)(tag & 0x07));
            }
        }

        return target;
    }
}