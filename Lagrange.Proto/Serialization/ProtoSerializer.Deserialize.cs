﻿using System.Diagnostics;
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

        while (!reader.IsCompleted)
        {
            uint tag = reader.DecodeVarIntUnsafe<uint>();
            if (objectInfo.Fields.TryGetValue(tag, out var fieldInfo))
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

        // polymorphic type
        if (converter.ObjectInfo.PolymorphicInfo?.PolymorphicIndicateIndex is > 0)
        {
            // has polymorphic type, read the first field to determine the actual type
            uint firstTag = reader.DecodeVarIntUnsafe<uint>();

            if (firstTag >>> 3 != converter.ObjectInfo.PolymorphicInfo.PolymorphicIndicateIndex)
            {
                ThrowHelper.ThrowInvalidOperationException_PolymorphicFieldNotFirst(typeof(T),
                    converter.ObjectInfo.PolymorphicInfo.PolymorphicIndicateIndex, firstTag >>> 3);
            }

            var firstField = converter.ObjectInfo.Fields[firstTag];
            firstField.Read(ref reader, boxed);
            var polyTypeKey = firstField.Get?.Invoke(boxed);
            if (polyTypeKey is null)
            {
                ThrowHelper.ThrowInvalidOperationException_FailedParsePolymorphicType(typeof(T), firstTag);
            }

            if (converter.ObjectInfo.PolymorphicInfo.GetTypeFromDiscriminator(polyTypeKey) is { } polyType)
            {
                (fieldInfos, var objectCreator ) = GetObjectInfoReflection<T>(polyType);
                target = objectCreator();
                boxed = target;
                if (boxed is null) ThrowHelper.ThrowInvalidOperationException_CanNotCreateObject(polyType);
            }
            else if (!converter.ObjectInfo.PolymorphicInfo.PolymorphicFallbackToBaseType)
            {
                ThrowHelper.ThrowInvalidOperationException_UnknownPolymorphicType(typeof(T), polyTypeKey);
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