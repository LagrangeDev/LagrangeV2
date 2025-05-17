using Lagrange.Proto.Nodes;
using Lagrange.Proto.Serialization;
using Lagrange.Proto.Utility;

namespace Lagrange.Proto.Test;

[TestFixture]
public class NodeTest
{
    private TestNode _obj;
    
    private byte[] _bytes;
    
    [SetUp]
    public void SetUp()
    {
        _obj = new TestNode
        {
            Test1 = 114514
        };

        _bytes = ProtoSerializer.Serialize(_obj);
    }
    
    [Test]
    public void TestParse()
    {
        var node = ProtoObject.Parse(_bytes);
        var test = node[1].GetValue<int>();
        var testArray = node[1].AsArray();
        
        Assert.Multiple(() =>
        {
            Assert.That(test, Is.EqualTo(_obj.Test1));
            Assert.That(testArray, Has.Count.EqualTo(1));
            Assert.That(testArray.Root, Is.SameAs(node));
        });
    }

    [Test]
    public void TestSerialize()
    {
        var node = ProtoObject.Parse(_bytes);
        var bytes = node.Serialize();

        Assert.That(bytes, Is.EqualTo(_bytes));
    }

    [Test]
    public void TestConstructor()
    {
        var node = new ProtoObject
        {
            { 1, new ProtoObject { { 1, 2 } } }, 
            { 1, new ProtoObject { { 1, 2 } } }, 
            { 3, 4 },
            { 5, 6 }
        };

        var bytes = node.Serialize();
        Console.WriteLine(Convert.ToHexString(bytes));
        var parsed = ProtoObject.Parse(bytes);

        int value = parsed[1][0][1].GetValue<int>();

        Assert.That(value, Is.EqualTo(2));
    }

    [Test]
    public void TestSerializeToWriter()
    {
        var node = new ProtoObject
        {
            { 1, new ProtoObject { { 1, 2 } } }, { 1, new ProtoObject { { 1, 2 } } }, { 3, 4 }, { 5, 6 }
        };

        var bufferWriter = new SegmentBufferWriter(1024);
        node.Serialize(bufferWriter);

        var bytes = bufferWriter.ToArray();
        var normalBytes = node.Serialize();
        
        Assert.That(bytes, Is.EqualTo(normalBytes));
    }
    
    [Test]
    public void TestHybrid()
    {
        var node = new TestNodeHybrid
        {
            Test1 = 114514,
            Test2 = new ProtoArray(WireType.VarInt, 1, 2, 3, 4, 5, 6, 7, 8, 9),
            Test3 = new ProtoObject
            {
                { 1, 2 }, { 3, 4 }
            }
        };

        var bytes = ProtoSerializer.SerializeProtoPackable(node);
        var parsed = ProtoSerializer.DeserializeProtoPackable<TestNodeHybrid>(bytes);

        Assert.Multiple(() =>
        {
            Assert.That(parsed.Test1, Is.EqualTo(node.Test1));
            Assert.That(parsed.Test2?.GetValues<int>(), Is.EqualTo(node.Test2.GetValues<int>()));
            Assert.That(parsed.Test3?[1].GetValue<int>(), Is.EqualTo(2));
            Assert.That(parsed.Test3?[3].GetValue<int>(), Is.EqualTo(4));
        });
    }

    [Test]
    public void TestOperators()
    {
        var node = new ProtoObject
        {
            { 1, true },
            { 1, (byte)1 },
            { 1, (sbyte)1 },
            { 1, (short)1 },
            { 1, (ushort)1 },
            { 1, (int)1 },
            { 1, (uint)1 },
            { 1, (long)1 },
            { 1, (ulong)1 },
            { 1, (bool?)true },
            { 1, (byte?)1 },
            { 1, (sbyte?)1 },
            { 1, (short?)1 },
            { 1, (ushort?)1 },
            { 1, (int?)1 },
            { 1, (uint?)1 },
            { 1, (long?)1 },
            { 1, (ulong?)1 },
            { 2, (float)1 },
            { 3, (double)1 },
            { 2, (float?)1 },
            { 3, (double?)1 },
            { 4, "Test" },
            { 4, "Test".AsMemory() },
            { 4, new byte[] { 1, 2, 3 } },
            { 4, (ReadOnlyMemory<byte>)new byte[] { 1, 2, 3 }.AsMemory() }
        };
        
        var bytes = node.Serialize();
        var parsed = ProtoObject.Parse(bytes);
        Assert.That(node[1].AsArray(), Has.Count.EqualTo(parsed[1].AsArray().GetValues<int>().ToArray().Length));
        Assert.That((bool)parsed[1][0], Is.EqualTo(true));
        Assert.That((bool)parsed[1][0].AsValue(), Is.EqualTo(true));
        Assert.That((byte)parsed[1][1], Is.EqualTo((byte)1));
        Assert.That((sbyte)parsed[1][2], Is.EqualTo((sbyte)1));
        Assert.That((short)parsed[1][3], Is.EqualTo((short)1));
        Assert.That((ushort)parsed[1][4], Is.EqualTo((ushort)1));
        Assert.That((int)parsed[1][5], Is.EqualTo((int)1));
        Assert.That((uint)parsed[1][6], Is.EqualTo((uint)1));
        Assert.That((long)parsed[1][7], Is.EqualTo((long)1));
        Assert.That((ulong)parsed[1][8], Is.EqualTo((ulong)1));
        Assert.That((float)parsed[2][0], Is.EqualTo(1f));
        Assert.That((double)parsed[3][0], Is.EqualTo(1d));
        Assert.That((string)parsed[4][0], Is.EqualTo("Test"));
        Assert.That((string)parsed[4][1], Is.EqualTo("Test"));
        Assert.That((byte[])parsed[4][2], Is.EqualTo(new byte[] { 1, 2, 3 }));
    }
}

[ProtoPackable]
public partial class TestNode
{
    [ProtoMember(1)] public int Test1 { get; set; }
}

[ProtoPackable]
public partial class TestNodeHybrid
{
    [ProtoMember(1)] public int Test1 { get; set; }
    
    [ProtoMember(2, NodesWireType = WireType.VarInt)] public ProtoArray? Test2 { get; set; }
    
    [ProtoMember(3)] public ProtoObject? Test3 { get; set; }
}