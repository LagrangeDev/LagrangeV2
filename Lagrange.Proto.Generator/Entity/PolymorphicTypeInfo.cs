using Microsoft.CodeAnalysis;

namespace Lagrange.Proto.Generator.Entity;

public class PolymorphicTypeInfo
{
    public INamedTypeSymbol PolymorphicKeyType { get; internal set; } = null!;

    public uint PolymorphicIndicateIndex { get; internal set; } = 0;
        
    public bool PolymorphicFallbackToBaseType { get; internal set; } = true;
        
    public List<PolymorphicDerivedTypeInfo> PolymorphicTypes { get; } = [];
}

public class PolymorphicDerivedTypeInfo
{
    public INamedTypeSymbol DerivedType { get; internal set; } = null!;
    public TypedConstant Key { get; internal set; }
}