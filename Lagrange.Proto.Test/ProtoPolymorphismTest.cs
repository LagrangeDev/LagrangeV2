using System.Reflection;
using Lagrange.Proto.Serialization;

namespace Lagrange.Proto.Test;

[TestFixture]
public class ProtoPolymorphismTest
{
    #region Basic Polymorphism

    [Test]
    public void ReflectionPolymorphism_SerializeBaseAndDeserializeBase_ReturnsCorrectDerivedType()
    {
        // Arrange
        BaseClass originalA = new DerivedClassA { NameProperty = "TestName" };
        BaseClass originalB = new DerivedClassB { ValueProperty = 114514f };

        byte[] bytesA = ProtoSerializer.Serialize(originalA);
        BaseClass deserializedA = ProtoSerializer.Deserialize<BaseClass>(bytesA);
        
        byte[] bytesB = ProtoSerializer.Serialize(originalB);
        BaseClass deserializedB = ProtoSerializer.Deserialize<BaseClass>(bytesB);

        Assert.Multiple(() =>
        {
            Assert.That(deserializedA, Is.AssignableTo<DerivedClassA>());
            Assert.That(deserializedA, Is.AssignableTo<BaseClass>());
            Assert.That(deserializedA.IdentifierProperty, Is.EqualTo(2));
            Assert.That(((DerivedClassA)deserializedA).NameProperty, Is.EqualTo("TestName"));
            
            Assert.That(deserializedB, Is.AssignableTo<DerivedClassB>());
            Assert.That(deserializedB, Is.AssignableTo<BaseClass>());
            Assert.That(deserializedB.IdentifierProperty, Is.EqualTo(3));
            Assert.That(((DerivedClassB)deserializedB).ValueProperty, Is.EqualTo(114514f));
            
        });
    }
    
    [Test]
    public void ReflectionPolymorphism_SerializeDerivedAndDeserializeBase_ReturnsCorrectDerivedType()
    {
        // Arrange
        DerivedClassA originalA = new DerivedClassA { NameProperty = "TestName" };
        DerivedClassB originalB = new DerivedClassB { ValueProperty = 114514f };

        byte[] bytesA = ProtoSerializer.Serialize(originalA);
        BaseClass deserializedA = ProtoSerializer.Deserialize<BaseClass>(bytesA);
        
        byte[] bytesB = ProtoSerializer.Serialize(originalB);
        BaseClass deserializedB = ProtoSerializer.Deserialize<BaseClass>(bytesB);

        Assert.Multiple(() =>
        {
            Assert.That(deserializedA, Is.AssignableTo<DerivedClassA>());
            Assert.That(deserializedA, Is.AssignableTo<BaseClass>());
            Assert.That(deserializedA.IdentifierProperty, Is.EqualTo(2));
            Assert.That(((DerivedClassA)deserializedA).NameProperty, Is.EqualTo("TestName"));

            Assert.That(deserializedB, Is.AssignableTo<DerivedClassB>());
            Assert.That(deserializedB, Is.AssignableTo<BaseClass>());
            Assert.That(deserializedB.IdentifierProperty, Is.EqualTo(3));
            Assert.That(((DerivedClassB)deserializedB).ValueProperty, Is.EqualTo(114514f));
        });
    }

    [Test]
    public void ReflectionPolymorphism_SerializeBaseAndDeserializeDerived_ReturnsCorrectDerivedType()
    {
        // Arrange
        BaseClass originalA = new DerivedClassA { NameProperty = "TestName" };
        BaseClass originalB = new DerivedClassB { ValueProperty = 114514f };

        byte[] bytesA = ProtoSerializer.Serialize(originalA);
        BaseClass deserializedA = ProtoSerializer.Deserialize<DerivedClassA>(bytesA);

        byte[] bytesB = ProtoSerializer.Serialize(originalB);
        BaseClass deserializedB = ProtoSerializer.Deserialize<DerivedClassB>(bytesB);

        Assert.Multiple(() =>
        {
            Assert.That(deserializedA, Is.AssignableTo<DerivedClassA>());
            Assert.That(deserializedA, Is.AssignableTo<BaseClass>());
            Assert.That(deserializedA.IdentifierProperty, Is.EqualTo(2));
            Assert.That(((DerivedClassA)deserializedA).NameProperty, Is.EqualTo("TestName"));

            Assert.That(deserializedB, Is.AssignableTo<DerivedClassB>());
            Assert.That(deserializedB, Is.AssignableTo<BaseClass>());
            Assert.That(deserializedB.IdentifierProperty, Is.EqualTo(3));
            Assert.That(((DerivedClassB)deserializedB).ValueProperty, Is.EqualTo(114514f));
        });
    }

    [Test]
    public void ReflectionPolymorphism_SerializeDerivedAndDeserializeDerived_ReturnsCorrectDerivedType()
    {
        // Arrange
        DerivedClassA originalA = new DerivedClassA { NameProperty = "TestName" };
        DerivedClassB originalB = new DerivedClassB { ValueProperty = 114514f };

        byte[] bytesA = ProtoSerializer.Serialize(originalA);
        BaseClass deserializedA = ProtoSerializer.Deserialize<DerivedClassA>(bytesA);

        byte[] bytesB = ProtoSerializer.Serialize(originalB);
        BaseClass deserializedB = ProtoSerializer.Deserialize<DerivedClassB>(bytesB);

        Assert.Multiple(() =>
        {
            Assert.That(deserializedA, Is.AssignableTo<DerivedClassA>());
            Assert.That(deserializedA, Is.AssignableTo<BaseClass>());
            Assert.That(deserializedA.IdentifierProperty, Is.EqualTo(2));
            Assert.That(((DerivedClassA)deserializedA).NameProperty, Is.EqualTo("TestName"));

            Assert.That(deserializedB, Is.AssignableTo<DerivedClassB>());
            Assert.That(deserializedB, Is.AssignableTo<BaseClass>());
            Assert.That(deserializedB.IdentifierProperty, Is.EqualTo(3));
            Assert.That(((DerivedClassB)deserializedB).ValueProperty, Is.EqualTo(114514f));
        });
    }

    #endregion

    #region ProtoPackable

    [Test]
    public void ProtoPackablePolymorphism_SerializeBaseAndDeserializeDerived_ReturnsCorrectDerivedType()
    {
        // Arrange
        BaseClass originalA = new DerivedClassA { NameProperty = "TestName" };
        BaseClass originalB = new DerivedClassB { ValueProperty = 114514f };

        byte[] bytesA = ProtoSerializer.SerializeProtoPackable(originalA);
        DerivedClassA deserializedA = ProtoSerializer.DeserializeProtoPackable<DerivedClassA>(bytesA);

        byte[] bytesB = ProtoSerializer.SerializeProtoPackable(originalB);
        DerivedClassB deserializedB = ProtoSerializer.DeserializeProtoPackable<DerivedClassB>(bytesB);

        Assert.Multiple(() =>
        {
            Assert.That(deserializedA, Is.AssignableTo<DerivedClassA>());
            Assert.That(deserializedA, Is.AssignableTo<BaseClass>());
            Assert.That(deserializedA.IdentifierProperty, Is.EqualTo(2));
            Assert.That(((DerivedClassA)deserializedA).NameProperty, Is.EqualTo("TestName"));

            Assert.That(deserializedB, Is.AssignableTo<DerivedClassB>());
            Assert.That(deserializedB, Is.AssignableTo<BaseClass>());
            Assert.That(deserializedB.IdentifierProperty, Is.EqualTo(3));
            Assert.That(((DerivedClassB)deserializedB).ValueProperty, Is.EqualTo(114514f));
        });
    }

    [Test]
    public void ProtoPackablePolymorphism_SerializeDerivedAndDeserializeDerived_ReturnsCorrectDerivedType()
    {
        // Arrange
        DerivedClassA originalA = new DerivedClassA { NameProperty = "TestName" };
        DerivedClassB originalB = new DerivedClassB { ValueProperty = 114514f };

        byte[] bytesA = ProtoSerializer.SerializeProtoPackable(originalA);
        BaseClass deserializedA = ProtoSerializer.DeserializeProtoPackable<DerivedClassA>(bytesA);

        byte[] bytesB = ProtoSerializer.SerializeProtoPackable(originalB);
        BaseClass deserializedB = ProtoSerializer.DeserializeProtoPackable<DerivedClassB>(bytesB);

        Assert.Multiple(() =>
        {
            Assert.That(deserializedA, Is.AssignableTo<DerivedClassA>());
            Assert.That(deserializedA, Is.AssignableTo<BaseClass>());
            Assert.That(deserializedA.IdentifierProperty, Is.EqualTo(2));
            Assert.That(((DerivedClassA)deserializedA).NameProperty, Is.EqualTo("TestName"));

            Assert.That(deserializedB, Is.AssignableTo<DerivedClassB>());
            Assert.That(deserializedB, Is.AssignableTo<BaseClass>());
            Assert.That(deserializedB.IdentifierProperty, Is.EqualTo(3));
            Assert.That(((DerivedClassB)deserializedB).ValueProperty, Is.EqualTo(114514f));
        });
    }

    [Test]
    public void ProtoPackablePolymorphism_SerializeBaseAndDeserializeBase_ReturnsCorrectDerivedType()
    {
        // Arrange
        BaseClass originalA = new DerivedClassA { NameProperty = "TestName" };
        BaseClass originalB = new DerivedClassB { ValueProperty = 114514f };

        byte[] bytesA = ProtoSerializer.SerializeProtoPackable(originalA);
        BaseClass deserializedA = ProtoSerializer.DeserializeProtoPackable<BaseClass>(bytesA);

        byte[] bytesB = ProtoSerializer.SerializeProtoPackable(originalB);
        BaseClass deserializedB = ProtoSerializer.DeserializeProtoPackable<BaseClass>(bytesB);

        Assert.Multiple(() =>
        {
            Assert.That(deserializedA, Is.AssignableTo<DerivedClassA>());
            Assert.That(deserializedA, Is.AssignableTo<BaseClass>());
            Assert.That(deserializedA.IdentifierProperty, Is.EqualTo(2));
            Assert.That(((DerivedClassA)deserializedA).NameProperty, Is.EqualTo("TestName"));

            Assert.That(deserializedB, Is.AssignableTo<DerivedClassB>());
            Assert.That(deserializedB, Is.AssignableTo<BaseClass>());
            Assert.That(deserializedB.IdentifierProperty, Is.EqualTo(3));
            Assert.That(((DerivedClassB)deserializedB).ValueProperty, Is.EqualTo(114514f));
        });
    }

    [Test]
    public void ProtoPackablePolymorphism_SerializeDerivedAndDeserializeBase_ReturnsCorrectDerivedType()
    {
        // Arrange
        DerivedClassA originalA = new DerivedClassA { NameProperty = "TestName" };
        DerivedClassB originalB = new DerivedClassB { ValueProperty = 114514f };

        byte[] bytesA = ProtoSerializer.SerializeProtoPackable(originalA);
        BaseClass deserializedA = ProtoSerializer.DeserializeProtoPackable<BaseClass>(bytesA);

        byte[] bytesB = ProtoSerializer.SerializeProtoPackable(originalB);
        BaseClass deserializedB = ProtoSerializer.DeserializeProtoPackable<BaseClass>(bytesB);

        Assert.Multiple(() =>
        {
            Assert.That(deserializedA, Is.AssignableTo<DerivedClassA>());
            Assert.That(deserializedA, Is.AssignableTo<BaseClass>());
            Assert.That(deserializedA.IdentifierProperty, Is.EqualTo(2));
            Assert.That(((DerivedClassA)deserializedA).NameProperty, Is.EqualTo("TestName"));

            Assert.That(deserializedB, Is.AssignableTo<DerivedClassB>());
            Assert.That(deserializedB, Is.AssignableTo<BaseClass>());
            Assert.That(deserializedB.IdentifierProperty, Is.EqualTo(3));
            Assert.That(((DerivedClassB)deserializedB).ValueProperty, Is.EqualTo(114514f));
        });
    }

    #endregion

    #region Nested Polymorphism Tests

    [Test]
    public void NestedPolymorphism_Reflection_SerializeAndDeserialize_ReturnsCorrectDerivedClassC()
    {
        // Arrange
        BaseClass baseRef =
            new DerivedClassC { NameProperty = "NestedName", Cannon = "BigCannon" };
        DerivedClassA classARef = (DerivedClassA)baseRef;
        DerivedClassC classCRef = (DerivedClassC)baseRef;

        // Serialize from BaseClass
        byte[] bytesFromBase = ProtoSerializer.Serialize(baseRef);
        var deserializedFromBase = ProtoSerializer.Deserialize<BaseClass>(bytesFromBase);

        // Serialize from DerivedClassA
        byte[] bytesFromA = ProtoSerializer.Serialize(classARef);
        var deserializedFromA = ProtoSerializer.Deserialize<DerivedClassA>(bytesFromA);

        // Serialize from DerivedClassC
        byte[] bytesFromC = ProtoSerializer.Serialize(classCRef);
        var deserializedFromC = ProtoSerializer.Deserialize<DerivedClassC>(bytesFromC);

        Assert.Multiple(() =>
        {
            Assert.That(deserializedFromBase, Is.AssignableTo<DerivedClassC>());
            Assert.That(((DerivedClassC)deserializedFromBase).NameProperty, Is.EqualTo("NestedName"));
            Assert.That(((DerivedClassC)deserializedFromBase).Cannon, Is.EqualTo("BigCannon"));
            Assert.That(((DerivedClassC)deserializedFromBase).AnotherIdentifier, Is.EqualTo(114514));

            Assert.That(deserializedFromA, Is.AssignableTo<DerivedClassC>());
            Assert.That(((DerivedClassC)deserializedFromA).NameProperty, Is.EqualTo("NestedName"));
            Assert.That(((DerivedClassC)deserializedFromA).Cannon, Is.EqualTo("BigCannon"));
            Assert.That(((DerivedClassC)deserializedFromA).AnotherIdentifier, Is.EqualTo(114514));

            Assert.That(deserializedFromC, Is.AssignableTo<DerivedClassC>());
            Assert.That(deserializedFromC.NameProperty, Is.EqualTo("NestedName"));
            Assert.That(deserializedFromC.Cannon, Is.EqualTo("BigCannon"));
            Assert.That(deserializedFromC.AnotherIdentifier, Is.EqualTo(114514));
        });
    }

    [Test]
    public void NestedPolymorphism_ProtoPackable_SerializeAndDeserialize_ReturnsCorrectDerivedClassC()
    {
        // Arrange
        BaseClass baseRef =
            new DerivedClassC { NameProperty = "NestedName", Cannon = "BigCannon" };
        DerivedClassA classARef = (DerivedClassA)baseRef;
        DerivedClassC classCRef = (DerivedClassC)baseRef;

        // Serialize from BaseClass
        byte[] bytesFromBase = ProtoSerializer.SerializeProtoPackable(baseRef);
        var deserializedFromBase = ProtoSerializer.DeserializeProtoPackable<BaseClass>(bytesFromBase);

        // Serialize from DerivedClassA
        byte[] bytesFromA = ProtoSerializer.SerializeProtoPackable(classARef);
        var deserializedFromA = ProtoSerializer.DeserializeProtoPackable<DerivedClassA>(bytesFromA);

        // Serialize from DerivedClassC
        byte[] bytesFromC = ProtoSerializer.SerializeProtoPackable(classCRef);
        var deserializedFromC = ProtoSerializer.DeserializeProtoPackable<DerivedClassC>(bytesFromC);

        Assert.Multiple(() =>
        {
            Assert.That(deserializedFromBase, Is.AssignableTo<DerivedClassC>());
            Assert.That(((DerivedClassC)deserializedFromBase).NameProperty, Is.EqualTo("NestedName"));
            Assert.That(((DerivedClassC)deserializedFromBase).Cannon, Is.EqualTo("BigCannon"));
            Assert.That(((DerivedClassC)deserializedFromBase).AnotherIdentifier, Is.EqualTo(114514));

            Assert.That(deserializedFromA, Is.AssignableTo<DerivedClassC>());
            Assert.That(((DerivedClassC)deserializedFromA).NameProperty, Is.EqualTo("NestedName"));
            Assert.That(((DerivedClassC)deserializedFromA).Cannon, Is.EqualTo("BigCannon"));
            Assert.That(((DerivedClassC)deserializedFromA).AnotherIdentifier, Is.EqualTo(114514));

            Assert.That(deserializedFromC, Is.AssignableTo<DerivedClassC>());
            Assert.That(deserializedFromC.NameProperty, Is.EqualTo("NestedName"));
            Assert.That(deserializedFromC.Cannon, Is.EqualTo("BigCannon"));
            Assert.That(deserializedFromC.AnotherIdentifier, Is.EqualTo(114514));
        });
    }

    #endregion
    
}

#region Test Classes

[ProtoPackable]
[ProtoPolymorphic(FieldNumber = 1)]
[ProtoDerivedType<int>(typeof(DerivedClassA), 2)]
[ProtoDerivedType<int>(typeof(DerivedClassB), 3)]
public partial class BaseClass
{
    public BaseClass() { }

    public BaseClass(int identifier)
    {
        IdentifierProperty = identifier;
    }

    [ProtoMember(1, NumberHandling = ProtoNumberHandling.Fixed32)] public int IdentifierProperty { get; set; } = -1;
}

[ProtoPackable]
[ProtoPolymorphic(FieldNumber = 4, FallbackToBaseType = true)]
[ProtoDerivedType<int>(typeof(DerivedClassC), 114514)]
public partial class DerivedClassA() : BaseClass(2)
{
    public DerivedClassA(int anotherIdentifier) : this()
    {
        AnotherIdentifier = anotherIdentifier;
    }

    [ProtoMember(4, NumberHandling = ProtoNumberHandling.Fixed32)] public int AnotherIdentifier { get; set; } = -1;
    [ProtoMember(2)] public string NameProperty { get; set; } = string.Empty;
}

[ProtoPackable]
public partial class DerivedClassC() : DerivedClassA(114514)
{
    [ProtoMember(10)] public string Cannon { get; set; } = string.Empty;
}

[ProtoPackable]
public partial class DerivedClassB() : BaseClass(3)
{
    [ProtoMember(2)] public float ValueProperty { get; set; } = 0f;
}

#endregion