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
        return T.DeserializeHandler(ref reader);
    }
    
    /// <summary>
    /// Deserialize the ProtoPackable Object from the source buffer, based on reflection
    /// </summary>
    /// <param name="data">The source buffer to read from</param>
    /// <typeparam name="T">The type of the object to deserialize</typeparam>
    /// <returns>The deserialized object</returns>
    [RequiresUnreferencedCode(SerializationUnreferencedCodeMessage)]
    [RequiresDynamicCode(SerializationRequiresDynamicCodeMessage)]
    public static T Deserialize<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T>(ReadOnlySpan<byte> data)
    {
        var reader = new ProtoReader(data);
        return DeserializeCore<T>(ref reader);
    }
    
    [RequiresUnreferencedCode(SerializationUnreferencedCodeMessage)]
    [RequiresDynamicCode(SerializationRequiresDynamicCodeMessage)]
    private static T DeserializeCore<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T>(ref ProtoReader reader)
    {
        ProtoObjectConverter<T> converter;
        if (ProtoTypeResolver.IsRegistered<T>())
        {
            if (ProtoTypeResolver.GetConverter<T>() as ProtoObjectConverter<T> is not { } c)
            {
                converter = new ProtoObjectConverter<T>(ProtoTypeResolver.CreateObjectInfo<T>());
                ProtoTypeResolver.Register(converter);
            }
            else
            {
                converter = c;
            }
        }
        else
        {
            ProtoTypeResolver.Register(converter = new ProtoObjectConverter<T>());
        }
        Debug.Assert(converter.ObjectInfo.ObjectCreator != null);

        T target = converter.ObjectInfo.ObjectCreator();
        var boxed = (object?)target; // avoid multiple times of boxing
        if (boxed is null) ThrowHelper.ThrowInvalidOperationException_CanNotCreateObject(typeof(T));

        while (!reader.IsCompleted)
        {
            uint tag = reader.DecodeVarIntUnsafe<uint>();
            if (converter.ObjectInfo.Fields.TryGetValue(tag, out var fieldInfo))
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