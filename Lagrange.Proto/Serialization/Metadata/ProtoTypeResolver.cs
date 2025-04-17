using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Lagrange.Proto.Serialization.Converter;

namespace Lagrange.Proto.Serialization.Metadata;

[SuppressMessage("ReSharper", "StaticMemberInGenericType")]
public static partial class ProtoTypeResolver
{
    private static readonly ConcurrentDictionary<Type, ProtoConverter> Converters = new(Environment.ProcessorCount, 150);
    
    static ProtoTypeResolver()
    {
        RegisterWellKnownTypes();
    }
    
    private static readonly Dictionary<Type, Type> AssignableConverters = new(3)
    {
        { typeof(Enum), typeof(ProtoEnumConverter<>) }
    };  

    private static readonly Dictionary<Type, Type> KnownGenericTypeConverters = new(3)
    {
        { typeof(Nullable<>), typeof(ProtoNullableConverter<>) },
    };
    
    private static readonly Dictionary<Type, Type> KnownGenericInterfaceConverters = new(3)
    {
        { typeof(IProtoSerializable<>), typeof(ProtoSerializableConverter<>) },
    };
    
    public static void Register<T>(ProtoConverter<T> converter, bool allowOverride = true)
    {
        if (!allowOverride && Check<T>.Registered) return;
        Check<T>.Registered = true; // avoid to call Cache() constructor called.
        Converters[typeof(T)] = converter;
        Cache<T>.Converter = converter;
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsRegistered<T>() => Check<T>.Registered;
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static ProtoConverter<T> GetConverter<T>() => Cache<T>.Converter;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static ProtoConverter GetConverter(Type type)
    {
        if (Converters.TryGetValue(type, out var converter))
        {
            return converter;
        }

        throw new NotImplementedException();
    }

    private static class Cache<T>
    {
        public static ProtoConverter<T> Converter = null!;
        
        static Cache()
        {
            if (Check<T>.Registered) return;

            var type = typeof(T);
            
            if (ResolveAssignableConverter<T>(type) is { } assignableConverter)
            {
                Converters[type] = assignableConverter;
                Converter = assignableConverter;
                Check<T>.Registered = true;
                return;
            }
            
            if (type.IsGenericType && ResolveGenericConverter<T>(type) is { } converter)
            {
                Converters[type] = converter;
                Converter = converter;
                Check<T>.Registered = true;
                return;
            }
            
            if (ResolveInterfaceConverter<T>(type) is { } interfaceConverter)
            {
                Converters[type] = interfaceConverter;
                Converter = interfaceConverter;
                Check<T>.Registered = true;
                return;
            }
            
            Converter = ResolveObjectConverter<T>();
            Check<T>.Registered = true;
        }
    }
    
    [UnconditionalSuppressMessage("Trimmer", "IL2055")]
    [UnconditionalSuppressMessage("Trimmer", "IL2067")]
    [UnconditionalSuppressMessage("Trimmer", "IL2072")]
    [UnconditionalSuppressMessage("Trimmer", "IL3050", Justification = "The generic type definition would always appear in metadata as it is a member in class serialized.")]
    private static ProtoConverter<T>? ResolveAssignableConverter<T>(Type type)
    {
        foreach (var (t, conv) in AssignableConverters)
        {
            if (t.IsAssignableFrom(type))
            {
                var converter = conv.MakeGenericType(type);
                return (ProtoConverter<T>)Activator.CreateInstance(converter)!;
            }
        }

        return null;
    }

    [UnconditionalSuppressMessage("Trimmer", "IL2055")]
    [UnconditionalSuppressMessage("Trimmer", "IL2067")]
    [UnconditionalSuppressMessage("Trimmer", "IL3050", Justification = "The generic type definition would always appear in metadata as it is a member in class serialized.")]
    private static ProtoConverter<T>? ResolveGenericConverter<T>(Type type)
    {
        if (KnownGenericTypeConverters.TryGetValue(type.GetGenericTypeDefinition(), out var converterType))
        {
            var args = type.GenericTypeArguments;
            var converter = converterType.MakeGenericType(args);
            return (ProtoConverter<T>)Activator.CreateInstance(converter)!;
        }

        return null;
    }
    
    [UnconditionalSuppressMessage("Trimmer", "IL2055")]
    [UnconditionalSuppressMessage("Trimmer", "IL2067")]
    [UnconditionalSuppressMessage("Trimmer", "IL2070", Justification = "The interface would always be preserve")]
    [UnconditionalSuppressMessage("Trimmer", "IL3050", Justification = "The generic type definition would always appear in metadata as it is a member in class serialized.")]
    private static ProtoConverter<T>? ResolveInterfaceConverter<T>(Type type)
    {
        var interfaces = type.GetInterfaces();
        
        foreach (var i in interfaces)
        {
            if (i.IsGenericType && KnownGenericInterfaceConverters.TryGetValue(i.GetGenericTypeDefinition(), out var converterType))
            {
                var args = i.GenericTypeArguments;
                var converter = converterType.MakeGenericType(args);
                return (ProtoConverter<T>)Activator.CreateInstance(converter)!;
            }
        }

        return null;
    }
    
    [UnconditionalSuppressMessage("Trimmer", "IL2026")]
    [UnconditionalSuppressMessage("Trimmer", "IL3050", Justification = "For AOT condition, the generic type definition would always appear in srcgen as it is a member in class serialized.")]
    private static ProtoConverter<T> ResolveObjectConverter<T>() => RuntimeFeature.IsDynamicCodeSupported ? new ProtoObjectConverter<T>() : new ProtoErrorConverter<T>();

    private static class Check<T>
    {
        public static bool Registered { get; set; }
    }
}