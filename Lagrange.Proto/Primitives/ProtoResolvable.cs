using System.Buffers;
using System.Runtime.CompilerServices;
using Lagrange.Proto.Serialization;
using Lagrange.Proto.Serialization.Metadata;

namespace Lagrange.Proto.Primitives;

public static class ProtoResolvableExtension
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void EncodeResolvable<T, TBufferWriter>(this ProtoWriter<TBufferWriter> writer, int field, WireType wireType, T value) where TBufferWriter : struct, IBufferWriter<byte>
    {
        var converter = ProtoTypeResolver.GetConverter<T>();
        converter.Write(field, wireType, writer, value);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void EncodeResolvable<T, TBufferWriter>(this ProtoWriter<TBufferWriter> writer, int field, WireType wireType, T value, ProtoNumberHandling numberHandling) where TBufferWriter : struct, IBufferWriter<byte>
    {
        var converter = ProtoTypeResolver.GetConverter<T>();
        converter.WriteWithNumberHandling(field, wireType, writer, value, numberHandling);
    }
}

public static class ProtoResolvableUtility
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Measure<T>(int field, WireType wireType, T value)
    {
        var converter = ProtoTypeResolver.GetConverter<T>();
        return converter.Measure(field, wireType, value);
    }
}