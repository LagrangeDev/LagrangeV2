using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using Lagrange.Proto.Primitives;
using Lagrange.Proto.Serialization.Converter;
using Lagrange.Proto.Serialization.Metadata;

namespace Lagrange.Proto.Serialization;

/// <summary>
/// Provides methods for serializing or deserializing objects from or to Protocol Buffers.
/// </summary>
public static partial class ProtoSerializer
{
    /// <summary>
    /// Serialize the ProtoPackable Object to the destination buffer, AOT Friendly, annotate the type with <see cref="ProtoPackableAttribute"/> to enable the source generator
    /// </summary>
    /// <param name="dest">The destination buffer to write to</param>
    /// <param name="obj">The object to serialize</param>
    /// <typeparam name="T">The type of the object to serialize</typeparam>
    public static void SerializeProtoPackable<T>(IBufferWriter<byte> dest, T obj) where T : IProtoSerializable<T>
    {
        var writer = ProtoWriterCache.RentWriter(dest);
        try
        {
            SerializeProtoPackableCore(writer, obj);
        }
        finally
        {
            ProtoWriterCache.ReturnWriter(writer);
        }
    }
    
    /// <summary>
    /// Serialize the ProtoPackable Object to a byte array, AOT Friendly, annotate the type with <see cref="ProtoPackableAttribute"/> to enable the source generator
    /// </summary>
    /// <param name="obj">The object to serialize</param>
    /// <returns>The serialized object as a byte array</returns>
    public static byte[] SerializeProtoPackable<T>(T obj) where T : IProtoSerializable<T>
    {
        var writer = ProtoWriterCache.RentWriterAndBuffer(512, out var buffer);
        try
        {
            SerializeProtoPackableCore(writer, obj);
            return buffer.ToArray();
        }
        finally
        {
            ProtoWriterCache.ReturnWriterAndBuffer(writer, buffer);
        }
    }
    
    private static void SerializeProtoPackableCore<T>(ProtoWriter writer, T obj) where T : IProtoSerializable<T>
    {
        if (!ProtoTypeResolver.IsRegistered<T>()) ProtoTypeResolver.Register(new ProtoSerializableConverter<T>());

        T.SerializeHandler(obj, writer);
        writer.Flush();
    }
    
    /// <summary>
    /// Serialize the object to the destination buffer, Reflection based
    /// </summary>
    /// <param name="dest">The destination buffer to write to</param>
    /// <param name="obj">The object to serialize</param>
    /// <typeparam name="T">The type of the object to serialize</typeparam>
    [RequiresUnreferencedCode(SerializationUnreferencedCodeMessage)]
    [RequiresDynamicCode(SerializationRequiresDynamicCodeMessage)]
    public static void Serialize<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T>(IBufferWriter<byte> dest, T obj) 
    {
        var writer = ProtoWriterCache.RentWriter(dest);
        try
        {
            SerializeCore(writer, obj);
        }
        finally
        {
            ProtoWriterCache.ReturnWriter(writer);
        }
    }
    
    /// <summary>
    /// Serialize the object to a byte array, Reflection based
    /// </summary>
    /// <param name="obj">The object to serialize</param>
    /// <returns>The serialized object as a byte array</returns>
    [RequiresUnreferencedCode(SerializationUnreferencedCodeMessage)]
    [RequiresDynamicCode(SerializationRequiresDynamicCodeMessage)]
    public static byte[] Serialize<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T>(T obj) 
    {
        var writer = ProtoWriterCache.RentWriterAndBuffer(512, out var buffer);
        try
        {
            SerializeCore(writer, obj);
            return buffer.ToArray();
        }
        finally
        {
            ProtoWriterCache.ReturnWriterAndBuffer(writer, buffer);
        }
    }

    [RequiresUnreferencedCode(SerializationUnreferencedCodeMessage)]
    [RequiresDynamicCode(SerializationRequiresDynamicCodeMessage)]
    private static void SerializeCore<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T>(ProtoWriter writer, T obj)
    {
        var converter = GetConverterOf<T>();
        var objectInfo = converter.ObjectInfo;
        object? boxed = obj; // avoid multiple times of boxing
        if (boxed is null) return;
        var fields = objectInfo.Fields;
        uint skipTag = 0;
        var polymorphicInfo = converter.ObjectInfo.PolymorphicInfo;
        // check polymorphic type
        if (polymorphicInfo?.PolymorphicIndicateIndex is > 0)
        {
            // has polymorphic type
            var index = polymorphicInfo.PolymorphicIndicateIndex;
            var fieldInfo = objectInfo.Fields.FirstOrDefault(t=>t.Value.Field == index);
            if (fieldInfo.Value is null) ThrowHelper.ThrowInvalidOperationException_NullPolymorphicDiscriminator(typeof(T));
            var discriminator = fieldInfo.Value.Get?.Invoke(boxed);
            if (discriminator is null) ThrowHelper.ThrowInvalidOperationException_NullPolymorphicDiscriminator(typeof(T));
            if (objectInfo.PolymorphicInfo!.GetTypeFromDiscriminator(discriminator) is not { } derivedTypeInfo)
            {
                ThrowHelper.ThrowInvalidOperationException_NullPolymorphicDiscriminator(typeof(T));
                return; // make compiler happy
            }
            skipTag = fieldInfo.Key;
            writer.EncodeVarInt(fieldInfo.Key);
            fieldInfo.Value.Write(writer, boxed);
            
            (fields, _, _) = GetObjectInfoReflection<T>(derivedTypeInfo);
        }
        
        foreach (var (tag, info) in fields)
        {
            if (skipTag != tag && info.ShouldSerialize(boxed, objectInfo.IgnoreDefaultFields))
            {
                writer.EncodeVarInt(tag);
                info.Write(writer, boxed);
            }
        }
        writer.Flush();
    }
}