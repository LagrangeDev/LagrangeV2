using System.Buffers;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using System.Text;
using Lagrange.Proto.Utility;

namespace Lagrange.Proto.Primitives;

[DebuggerDisplay("{DebuggerDisplay,nq}")]
public class ProtoWriter : IDisposable
{
    private static readonly Vector128<sbyte> Ascend = Vector128.Create(0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15);
    
    private const int DefaultGrowthSize = 1024;
    private const int InitialGrowthSize = DefaultGrowthSize >> 4;
    
    private Memory<byte> _memory;
    private IBufferWriter<byte>? _writer;
    
    public int BytesPending { get; private set; }
    
    public long BytesCommitted { get; private set; }

    public ProtoWriter() { }
    
    public ProtoWriter(IBufferWriter<byte> writer)
    {
        _writer = writer;
        _memory = default;
    }

    public void EncodeString(ReadOnlySpan<char> str)
    {
        int count = ProtoHelper.GetVarIntLength(str.Length);
        int min = ProtoHelper.GetVarIntMin(count);
        int max = ProtoHelper.GetVarIntMax(count);
        int utf16Max = ProtoConstants.MaxExpansionFactorWhileTranscoding * str.Length;
        if (_memory.Length < utf16Max) Grow(utf16Max);
        
        if (str.Length > min && str.Length < max) // falls within the range
        {
            BytesPending += count;
            var status = ProtoWriteHelper.ToUtf8(str, _memory.Span[BytesPending..], out int written);
            Debug.Assert(status == OperationStatus.Done);
            BytesPending += written;

            ref byte dest = ref Unsafe.AddByteOffset(ref MemoryMarshal.GetReference(_memory.Span), BytesPending - written - count);
            EncodeVarIntLengthTo(written, ref dest);
        }
        else
        {
            EncodeVarInt(Encoding.UTF8.GetByteCount(str));
            var status = ProtoWriteHelper.ToUtf8(str, _memory.Span[BytesPending..], out int written);
            Debug.Assert(status == OperationStatus.Done);
            BytesPending += written;
        }
    }
    
    public void EncodeBytes(ReadOnlySpan<byte> bytes)
    {
        int length = bytes.Length;
        if (_memory.Length - BytesPending < length) Grow(length);
        
        EncodeVarInt(length);
        bytes.CopyTo(_memory.Span[BytesPending..]);
        BytesPending += length;
    }
    
    public void EncodeFixed32<T>(T value) where T : unmanaged, INumber<T>
    {
        if (_memory.Length - BytesPending < 4) Grow(4);
        
        ref byte destination = ref MemoryMarshal.GetReference(_memory.Span);
        Unsafe.As<byte, T>(ref Unsafe.Add(ref destination, BytesPending)) = value;
        BytesPending += 4;
    }
    
    public void EncodeFixed64<T>(T value) where T : unmanaged, INumber<T>
    {
        if (_memory.Length - BytesPending < 8) Grow(8);
        
        ref byte destination = ref MemoryMarshal.GetReference(_memory.Span);
        Unsafe.As<byte, T>(ref Unsafe.Add(ref destination, BytesPending)) = value;
        BytesPending += 8;
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void EncodeVarInt<T>(T value) where T : unmanaged, INumber<T>
    {
        if (_memory.Length - BytesPending >= 10)
        {
            if (value < T.CreateTruncating(0x80))
            {
                Unsafe.Add(ref MemoryMarshal.GetReference(_memory.Span), BytesPending++) = byte.CreateTruncating(value);
                return;
            }
            EncodeVarIntUnsafe(value);
            return;
        }
        EncodeVarIntSlowPath(value);
    }
    
    [MethodImpl(MethodImplOptions.NoInlining)]
    private void EncodeVarIntSlowPath<T>(T value) where T : unmanaged, INumber<T>
    {
        ulong v = ulong.CreateTruncating(value);
        
        while (v > 127)
        {
            WriteRawByte((byte)((v & 0x7F) | 0x80));
            v >>= 7;
        }
        WriteRawByte((byte)v);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void EncodeVarIntLengthTo(int value, ref byte dest)
    {
        if (value < 0x80) 
        {
            dest = (byte)value;
            return;
        }
        
        ref byte first = ref dest;
        int position = 0;
        
        while (true)
        {
            if (value < 0x80)
            {
                Unsafe.Add(ref first, position) = (byte)value;
                break;
            }
            Unsafe.Add(ref first, position) = (byte)(value | 0x80);
            position++;
            value >>= 7;
        }
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteRawByte(byte value)
    {
        if (_memory.Length - BytesPending < 1) Grow(1);
        
        Unsafe.Add(ref MemoryMarshal.GetReference(_memory.Span), BytesPending++) = value;
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteRawBytes(ReadOnlySpan<byte> bytes)
    {
        if (_memory.Length - BytesPending < bytes.Length) Grow(bytes.Length);
        
        bytes.CopyTo(_memory.Span[BytesPending..]);
        BytesPending += bytes.Length;
    }
    
    /// <summary>
    /// Max VarInt Bytes for u8: 2
    /// Max VarInt Bytes for u16: 3
    /// Max VarInt Bytes for u32: 5
    /// Max VarInt Bytes for u64: 10
    /// Use sizeof(T) to ensure JIT optimization
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private unsafe void EncodeVarIntUnsafe<T>(T value) where T : unmanaged, INumberBase<T>
    {
        if (!Sse2.IsSupported) throw new PlatformNotSupportedException();
        
        ulong v = ulong.CreateTruncating(value);

        if (sizeof(T) <= 4)
        {
            ulong stage1 = PackScalar<T>(v);
            int leading = BitOperations.LeadingZeroCount(stage1);
            int bytesNeeded = 8 - ((leading - 1) >> 3);
            
            ulong msbMask = 0xFFFFFFFFFFFFFFFF >> ((8 - bytesNeeded + 1) * 8 - 1);
            ulong merged = stage1 | (0x8080808080808080 & msbMask);
            
            ref byte destination = ref MemoryMarshal.GetReference(_memory.Span);
            Unsafe.As<byte, ulong>(ref Unsafe.Add(ref destination, BytesPending)) = merged;
            BytesPending += bytesNeeded;
        }
        else
        {
            var stage1 = PackVector<T>(v).AsSByte();
            var minimum = Vector128.Create(-1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
            var exists = Sse2.Or(Sse2.CompareGreaterThan(stage1, Vector128<sbyte>.Zero), minimum);
            uint bits = (uint)Sse2.MoveMask(exists);
            
            byte bytes = (byte)(32 - BitOperations.LeadingZeroCount(bits));
            var mask = Sse2.CompareLessThan(Ascend, Vector128.Create((sbyte)bytes));
            
            var shift = Sse2.ShiftRightLogical128BitLane(mask, 1);
            var msbmask = Sse2.And(shift, Vector128.Create((sbyte)-128));
            var merged = Sse2.Or(stage1, msbmask);
            
            ref byte destination = ref MemoryMarshal.GetReference(_memory.Span);
            Unsafe.As<byte, Vector128<sbyte>>(ref Unsafe.Add(ref destination, BytesPending)) = merged;
            BytesPending += bytes;
        }
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static unsafe ulong PackScalar<T>(ulong v) where T : unmanaged, INumberBase<T>
    {
        if (Bmi2.X64.IsSupported)
        {
            return sizeof(T) switch
            {
                sizeof(byte) => Bmi2.X64.ParallelBitDeposit(v, 0x000000000000017f),
                sizeof(ushort) => Bmi2.X64.ParallelBitDeposit(v, 0x0000000000037f7f),
                sizeof(uint) => Bmi2.X64.ParallelBitDeposit(v, 0x0000000f7f7f7f7f),
                _ => throw new NotSupportedException()
            };
        }
        else
        {
            return sizeof(T) switch
            {
                sizeof(byte) => (v & 0x000000000000007f) | ((v & 0x0000000000000080) << 1),
                sizeof(ushort) => (v & 0x000000000000007f) | ((v & 0x0000000000003f80) << 1) | ((v & 0x000000000000c000) << 2),
                sizeof(uint) => (v & 0x000000000000007f) | ((v & 0x0000000000003f80) << 1) | ((v & 0x00000000001fc000) << 2) | ((v & 0x000000000fe00000) << 3) | ((v & 0x00000000f0000000) << 4),
                _ => throw new NotSupportedException()
            };
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static unsafe Vector128<ulong> PackVector<T>(ulong v) where T : unmanaged, INumberBase<T>
    {
        if (sizeof(T) < 8) throw new InvalidOperationException("Vector is too small");
        
        ulong x, y;
        if (Bmi2.X64.IsSupported)
        {
            x = Bmi2.X64.ParallelBitDeposit(v, 0x7f7f7f7f7f7f7f7f);
            y = Bmi2.X64.ParallelBitDeposit(v >> 56, 0x000000000000017f);
        }
        else if (Avx2.IsSupported)
        {
            var b = Vector128.Create(v);
            var c = Sse2.Or(
                Sse2.Or(
                    Avx2.ShiftLeftLogicalVariable(Sse2.And(b, Vector128.Create(0x00000007f0000000ul, 0x000003f800000000ul)), Vector128.Create(4ul, 5ul)),
                    Avx2.ShiftLeftLogicalVariable(Sse2.And(b, Vector128.Create(0x0001fc0000000000ul, 0x00fe000000000000ul)), Vector128.Create(6ul, 7ul))
                ),
                Sse2.Or(
                    Avx2.ShiftLeftLogicalVariable(Sse2.And(b, Vector128.Create(0x000000000000007ful, 0x0000000000003f80ul)), Vector128.Create(0ul, 1ul)),
                    Avx2.ShiftLeftLogicalVariable(Sse2.And(b, Vector128.Create(0x00000000001fc000ul, 0x000000000fe00000ul)), Vector128.Create(2ul, 3ul))
                )
            );
            var d = Sse2.Or(c, Sse2.ShiftRightLogical128BitLane(c, 8));
            x = Sse41.X64.Extract(d, 0);
            y = (v & 0x7f00000000000000) >> 56 | (v & 0x8000000000000000) >> 55;
        }
        else
        {
            x = (v & 0x000000000000007f) | ((v & 0x0000000000003f80) << 1) | ((v & 0x00000000001fc000) << 2) | ((v & 0x000000000fe00000) << 3) | ((v & 0x00000007f0000000) << 4) | ((v & 0x000003f800000000) << 5) | ((v & 0x0001fc0000000000) << 6) | ((v & 0x00fe000000000000) << 7);
            y = ((v & 0x7f00000000000000) >> 56) | ((v & 0x8000000000000000) >> 55);
        }

        return Vector128.Create(x, y);
    }
    
    [MethodImpl(MethodImplOptions.NoInlining)]
    private void Grow(int requiredSize)
    {
        Debug.Assert(requiredSize > 0);

        if (_memory.Length == 0)
        {
            FirstCallToGetMemory(requiredSize);
            return;
        }

        int sizeHint = Math.Max(DefaultGrowthSize, requiredSize);

        Debug.Assert(BytesPending != 0);
        Debug.Assert(_writer != null);

        _writer.Advance(BytesPending);
        BytesCommitted += BytesPending;
        BytesPending = 0;

        _memory = _writer.GetMemory(sizeHint);

        if (_memory.Length < sizeHint) ThrowHelper.ThrowInvalidOperationException_NeedLargerSpan();
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private void FirstCallToGetMemory(int requiredSize)
    {
        Debug.Assert(_memory.Length == 0);
        Debug.Assert(BytesPending == 0);
        Debug.Assert(_writer != null);
        
        int sizeHint = Math.Max(InitialGrowthSize, requiredSize);
        _memory = _writer.GetMemory(sizeHint);

        if (_memory.Length < sizeHint) ThrowHelper.ThrowInvalidOperationException_NeedLargerSpan();
    }
    
    public void Flush()
    {
        CheckNotDisposed();
        _memory = default;
        
        Debug.Assert(_writer != null);
        if (BytesPending != 0)
        {
            _writer.Advance(BytesPending);
            BytesCommitted += BytesPending;
            BytesPending = 0;
        }
    }
    
    public void Reset(IBufferWriter<byte> bufferWriter)
    {
        _writer = bufferWriter ?? throw new ArgumentNullException(nameof(bufferWriter));

        ResetHelper();
    }
    
    internal void ResetAllStateForCacheReuse()
    {
        ResetHelper();

        _writer = null;
    }
    
    private void ResetHelper()
    {
        BytesPending = 0;
        BytesCommitted = 0;
        _memory = default;
    }
    
    private void CheckNotDisposed()
    {
        if (_writer == null)
        {
            ThrowHelper.ThrowObjectDisposedException_ProtoWriter();
        }
    }
    
    public void Dispose()
    {
        if (_writer == null) return;

        Flush();
        ResetHelper();

        _writer = null;
    }
    
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private string DebuggerDisplay => $"BytesCommitted = {BytesCommitted} BytesPending = {BytesPending}";
}