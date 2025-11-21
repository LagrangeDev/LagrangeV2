using Lagrange.Proto.Nodes;

namespace Lagrange.Proto.Runner;

internal static partial class Program
{
    private static void Main(string[] args)
    {
        // Run VarInt performance test
        VarIntPerformanceTest.Run();

        Console.WriteLine("\n=== Original ProtoObject Test ===");
        var test = new ProtoObject()
        {
            { 1, new ProtoObject{ { 1, 2 } } },
            { 1, new ProtoObject{ { 1, 2 } } },
            { 3, 4 },
            { 5, 6 }
        };

        var bytes = test.Serialize();
        Console.WriteLine(Convert.ToHexString(bytes));
        var parsed = ProtoObject.Parse(bytes);

        int value = parsed[1][0][1].GetValue<int>();
        Console.WriteLine($"Parsed value: {value}");
    }
}

#region Test Classes
[ProtoPackable]
[ProtoPolymorphic(FieldNumber = 1)]
[ProtoDerivedType<int>(typeof(DerivedClassA), 4)]
[ProtoDerivedType<int>(typeof(DerivedClassB), 3)]
public partial class BaseClass
{
    public BaseClass() : this(-1) { }

    public BaseClass(int identifier)
    {
        IdentifierProperty = identifier;
    }

    [ProtoMember(1)] public int IdentifierProperty { get; set; }
}


[ProtoPackable]
public partial class DerivedClassA() : BaseClass(2)
{
    [ProtoMember(2)] public string? NameProperty { get; set; }
}


[ProtoPackable]
public partial class DerivedClassB() : BaseClass(3)
{
    [ProtoMember(2)] public float ValueProperty { get; set; } = 0f;
}

#endregion