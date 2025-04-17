using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;

namespace Lagrange.Proto.Utility;

public static class ProtoHelper
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static int GetVarIntMin(int length) => length switch
    {
        1 => (1 << 0),
        2 => (1 << 7),
        3 => (1 << 14),
        4 => (1 << 21),
        _ => throw new ArgumentOutOfRangeException(nameof(length), "Invalid length for VarInt.")
    };
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static int GetVarIntMax(int length) => length switch
    {
        1 => (1 << 7) - 1,
        2 => (1 << 14) - 1,
        3 => (1 << 21) - 1,
        4 => (1 << 28) - 1,
        _ => throw new ArgumentOutOfRangeException(nameof(length), "Invalid length for VarInt.")
    };

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int GetVarIntLength<T>(T value) where T : unmanaged, INumberBase<T>
    {
        ulong v = ulong.CreateTruncating(value);
        return BitOperations.TrailingZeroCount(v) / 7 + 1;
    }
    
    public static unsafe T ZigZagEncode<T>(T value) where T : unmanaged, INumber<T>
    {
        if (sizeof(T) < 4)
        {
            int v = int.CreateSaturating(value);
            return T.CreateTruncating((v << 1) ^ (v >> 31));
        }
        else
        {
            long v = long.CreateSaturating(value);
            return T.CreateTruncating((v << 1) ^ (v >> 63));
        }
    }
    
    public static unsafe T ZigZagDecode<T>(T value) where T : unmanaged, INumber<T>
    {
        if (sizeof(T) < 4)
        {
            int v = int.CreateSaturating(value);
            return T.CreateTruncating((v >> 1) ^ -(v & 1));
        }
        else
        {
            long v = long.CreateSaturating(value);
            return T.CreateTruncating((v >> 1) ^ -(v & 1));
        }
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int CountString(ReadOnlySpan<char> str)
    {
        int length = Encoding.UTF8.GetByteCount(str);
        return GetVarIntLength(length) + length;
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int CountBytes(ReadOnlySpan<byte> str)
    {
        return GetVarIntLength(str.Length) + str.Length;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int CountProtoPackable<T>(T obj) where T : IProtoSerializable<T>
    {
        int length = T.MeasureHandler(obj);
        return GetVarIntLength(length) + length;
    }
}