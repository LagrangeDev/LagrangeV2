using System.Buffers;
using System.Runtime.Intrinsics.Arm;
using System.Runtime.Intrinsics.X86;
using Lagrange.Proto.Primitives;

namespace Lagrange.Proto.Test;

[TestFixture]
public class EncoderTest
{
    private long _number1;

    private int _number2;

    private int _number3;

    private byte[] _longInt;
    
    private byte[] _twoInt;
    
    [SetUp]
    public void Setup()
    {
        _number1 = 1145141919810;
        _number2 = 114514;
        _number3 = 1919810;

        var buffer = new ArrayBufferWriter<byte>();
        var writer = new ProtoWriter(buffer);
        
        writer.EncodeVarInt(_number1);
        writer.EncodeVarInt(_number2);
        
        writer.Flush();
        _longInt = buffer.WrittenMemory.ToArray();
        buffer.Clear();
        
        writer.EncodeVarInt(_number2);
        writer.EncodeVarInt(_number3);
        
        writer.Flush();
        _twoInt = buffer.WrittenMemory.ToArray();
        buffer.Clear();
    }
    
    [Test]
    public void TestTwoInt()
    {
        var reader = new ProtoReader(_twoInt);
        int number1 = reader.DecodeVarInt<int>();
        int number2 = reader.DecodeVarInt<int>();
        
        Assert.Multiple(() =>
        {
            Assert.That(number1, Is.EqualTo(_number2));
            Assert.That(number2, Is.EqualTo(_number3));
        });
    }
    
    [Test]
    public void TestLongInt()
    {
        var reader = new ProtoReader(_longInt);
        long number1 = reader.DecodeVarInt<long>();
        long number2 = reader.DecodeVarInt<long>();
        
        Assert.Multiple(() =>
        {
            Assert.That(number1, Is.EqualTo(_number1));
            Assert.That(number2, Is.EqualTo(_number2));
        });
    }
    
    [Test]
    public void TestReadTwoInt()
    {
        if (!Sse3.IsSupported && !AdvSimd.Arm64.IsSupported) Assert.Ignore("SSSE3/NEON is not supported on this platform.");

        var reader = new ProtoReader(_twoInt);
        var (number1, number2) = reader.DecodeVarIntUnsafe<int, int>(_twoInt);
        
        Assert.Multiple(() =>
        {
            Assert.That(number1, Is.EqualTo(_number2));
            Assert.That(number2, Is.EqualTo(_number3));
        });
    }
    
    [Test]
    public void TestReadLongInt()
    {
        if (!Sse3.IsSupported && !AdvSimd.Arm64.IsSupported) Assert.Ignore("SSSE3/NEON is not supported on this platform.");
        
        var reader = new ProtoReader(_longInt);
        var (number1, number2) = reader.DecodeVarIntUnsafe<long, int>(_longInt);
        
        Assert.Multiple(() =>
        {
            Assert.That(number1, Is.EqualTo(_number1));
            Assert.That(number2, Is.EqualTo(_number2));
        });
    }

    [Test]
    public void TestUnsafeRead()
    {
        if (!Sse3.IsSupported && !AdvSimd.Arm64.IsSupported) Assert.Ignore("SSSE3/NEON is not supported on this platform.");

        Span<byte> longerBuffer = stackalloc byte[256];
        _longInt.AsSpan().CopyTo(longerBuffer);
        
        var reader = new ProtoReader(longerBuffer);
        var (number1, number2) = reader.DecodeVarIntUnsafe<long, int>(longerBuffer);
        
        reader = new ProtoReader(longerBuffer);
        long number12 = reader.DecodeVarInt<long>();
        int number22 = reader.DecodeVarInt<int>();
        
        Assert.Multiple(() =>
        {
            Assert.That(number1, Is.EqualTo(_number1));
            Assert.That(number2, Is.EqualTo(_number2));
            Assert.That(number12, Is.EqualTo(_number1));
            Assert.That(number22, Is.EqualTo(_number2));
        });
    }
}