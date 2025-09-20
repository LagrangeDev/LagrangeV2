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
}

#region Test Classes
[ProtoPackable]
[ProtoPolymorphic(FieldNumber = 1)]
[ProtoDerivedType<int>(typeof(DerivedClassA), 2)]
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