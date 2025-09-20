using Lagrange.Proto.Generator.Entity;
using Lagrange.Proto.Generator.Utility;
using Lagrange.Proto.Generator.Utility.Extension;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Lagrange.Proto.Generator;

public partial class ProtoSourceGenerator
{
    private partial class Emitter
    {
        private const string TypeInfoFieldName = "_typeInfo";
        private const string TypeInfoPropertyName = "TypeInfo";
        
        private const string ProtoObjectInfoTypeRef = "global::Lagrange.Proto.Serialization.Metadata.ProtoObjectInfo<{0}>";
        private const string ProtoPolymorphicObjectInfoTypeRef = "global::Lagrange.Proto.Serialization.Metadata.ProtoPolymorphicObjectInfo<{0}>";
        private const string ProtoPolymorphicDerivedTypeDescriptorTypeRef = "global::Lagrange.Proto.Serialization.Metadata.ProtoPolymorphicDerivedTypeDescriptor<{0}>";
        private const string ProtoFieldInfoTypeRef = "global::Lagrange.Proto.Serialization.Metadata.ProtoFieldInfo";
        private const string ProtoFieldInfoGenericTypeRef = "global::Lagrange.Proto.Serialization.Metadata.ProtoFieldInfo<{0}>";
        private const string ProtoMapFieldInfoGenericTypeRef = "global::Lagrange.Proto.Serialization.Metadata.ProtoMapFieldInfo<{0}, {1}, {2}>";
        private const string ProtoTypeResolverTypeRef = "global::Lagrange.Proto.Serialization.Metadata.ProtoTypeResolver";
        private const string WireTypeTypeRef = "global::Lagrange.Proto.Serialization.WireType";
        private const string ConverterTypeRef = "global::Lagrange.Proto.Serialization.Converter";
        private const string GenericTypeRef = "System.Collections.Generic";
        
        private const string IsRegisteredMethodRef = ProtoTypeResolverTypeRef + ".IsRegistered<{0}>";
        private const string RegisterMethodRef = ProtoTypeResolverTypeRef + ".Register({0})";
        
        private string ProtoObjectInfoTypeRefGeneric => string.Format(ProtoObjectInfoTypeRef, _fullQualifiedName);

        private static readonly List<(Func<ITypeSymbol, bool>, Func<ITypeSymbol, string>, string)> Converters =
        [
            (x => x.IsValueType && x.IsNullable(), x => SymbolResolver.GetGenericTypeNonNull(x).GetFullName(), "ProtoNullableConverter<{0}>"),
            (x => x.TypeKind == TypeKind.Enum, x => x.GetFullName(), "ProtoEnumConverter<{0}>"),
            (x => x is IArrayTypeSymbol, x => SymbolResolver.GetGenericTypeNonNull(x).GetFullName(), "ProtoArrayConverter<{0}>"),
            (x => x is INamedTypeSymbol { IsGenericType: true, ConstructedFrom.Name: "List" } s && s.ContainingNamespace.ToString() == GenericTypeRef, x => SymbolResolver.GetGenericTypeNonNull(x).GetFullName(), "ProtoListConverter<{0}>"),
            (x => SymbolResolver.IsProtoPackable(x), x => x.GetFullName(), "ProtoSerializableConverter<{0}>"),
        ];
        
        private void EmitTypeInfo(SourceWriter source)
        {
            source.WriteLine("#pragma warning disable CS0108");
            source.WriteLine();
            source.WriteLine($"public static {ProtoObjectInfoTypeRefGeneric}? {TypeInfoFieldName};");
            source.WriteLine();
            
            source.WriteLine($"public static {ProtoObjectInfoTypeRefGeneric} {TypeInfoPropertyName} => {TypeInfoFieldName} ??= GetTypeInfo();");
            source.WriteLine();
            source.WriteLine("#pragma warning restore CS0108");
            source.WriteLine();
            
            source.WriteLine($"private static {ProtoObjectInfoTypeRefGeneric} GetTypeInfo()");
            source.WriteLine('{');
            source.Indentation++;
            
            foreach (var info in parser.Fields.Select(kv => kv.Value))
            {
                if (info.TypeSymbol is INamedTypeSymbol { IsGenericType: true } genericType) // resolve nested generic types
                {
                    foreach (var arg in genericType.TypeArguments) EmitByTypeSymbol(source, arg);
                }
                
                EmitByTypeSymbol(source, info.TypeSymbol);
            }
            
            source.WriteLine($"return new {ProtoObjectInfoTypeRefGeneric}()");
            source.WriteLine('{');
            source.Indentation++;
            
            source.WriteLine($"Fields = new global::System.Collections.Generic.Dictionary<uint, {string.Format(ProtoFieldInfoTypeRef)}>()");
            source.WriteLine('{');
            source.Indentation++;
            foreach (var kv in parser.Fields)
            {
                int field = kv.Key;
                var info = kv.Value;

                if (info.ExtraTypeInfo.Count == 0) EmitFieldInfo(source, field, info);
                else EmitMapFieldInfo(source, field, info);
            }
            source.Indentation--;
            source.WriteLine("},");
            
            source.WriteLine($"ObjectCreator = () => new {_fullQualifiedName}(),");
            EmitPolymorphicInfo(source, parser.PolymorphicInfo);
            source.WriteLine($"IgnoreDefaultFields = {parser.IgnoreDefaultFields.ToString().ToLower()}");
            
            source.Indentation--;
            source.WriteLine("};");
            source.Indentation--;
            source.WriteLine('}');

            EmitPolymorphicDerivedTypeDescriptor(source, parser.PolymorphicInfo);
            
            return;
            
            static void EmitByTypeSymbol(SourceWriter source, ITypeSymbol typeSymbol)
            {
                foreach (var kv2 in Converters)
                {
                    var predicate = kv2.Item1;
                    string typeName = kv2.Item2(typeSymbol);
                    string converter = ConverterTypeRef + "." + kv2.Item3;
                    
                    if (predicate(typeSymbol))
                    {
                        source.WriteLine($"if (!{string.Format(IsRegisteredMethodRef, typeSymbol.GetFullName())}())");
                        source.WriteLine('{');
                        source.Indentation++;
                        source.WriteLine($"{string.Format(RegisterMethodRef, string.Format("new " + converter + "()", typeName))};");
                        source.Indentation--;
                        source.WriteLine('}');
                        source.WriteLine();
                    }
                }
            }
        }

        private void EmitFieldInfo(SourceWriter source, int field, ProtoFieldInfo info)
        {
            int tag = field << 3 | (byte)info.WireType;
            
            source.WriteLine($"[{tag}] = new {string.Format(ProtoFieldInfoGenericTypeRef, info.TypeSymbol.GetFullName())}({field}, {WireTypeTypeRef}.{info.WireType}, typeof({_fullQualifiedName}))");
            source.WriteLine('{');
            source.Indentation++;
            
            source.WriteLine($"Get = {ObjectVarName} => (({_fullQualifiedName}){ObjectVarName}).{info.Symbol.Name},");
            source.WriteLine($"Set = ({ObjectVarName}, {ValueVarName}) => (({_fullQualifiedName}){ObjectVarName}).{info.Symbol.Name} = {ValueVarName},");
            source.WriteLine($"NumberHandling = {ProtoNumberHandlingTypeRef}.{(info.IsSigned ? "Signed" : "Default")}");
            
            source.Indentation--;
            source.WriteLine("},");
        }
        
        private void EmitMapFieldInfo(SourceWriter source, int field, ProtoFieldInfo info)
        {
            int tag = field << 3 | (byte)info.WireType;
            
            var key = info.ExtraTypeInfo[0];
            var value = info.ExtraTypeInfo[1];

            string type = string.Format(ProtoMapFieldInfoGenericTypeRef, info.TypeSymbol.GetFullName(), key.TypeSymbol.GetFullName(), value.TypeSymbol.GetFullName());
            source.WriteLine($"[{tag}] = new {type}({field}, {WireTypeTypeRef}.{key.WireType}, {WireTypeTypeRef}.{value.WireType}, typeof({_fullQualifiedName}))");
            source.WriteLine('{');
            source.Indentation++;
            
            source.WriteLine($"Get = {ObjectVarName} => (({_fullQualifiedName}){ObjectVarName}).{info.Symbol.Name},");
            source.WriteLine($"Set = ({ObjectVarName}, {ValueVarName}) => (({_fullQualifiedName}){ObjectVarName}).{info.Symbol.Name} = {ValueVarName},");
            source.WriteLine($"NumberHandling = {ProtoNumberHandlingTypeRef}.{(key.IsSigned ? "Signed" : "Default")},");
            source.WriteLine($"ValueNumberHandling = {ProtoNumberHandlingTypeRef}.{(value.IsSigned ? "Signed" : "Default")}");
            
            source.Indentation--;
            source.WriteLine("},");
        }

        private void EmitPolymorphicInfo(SourceWriter source, PolymorphicTypeInfo polymorphicInfo)
        {
            if (polymorphicInfo.PolymorphicIndicateIndex == 0) return;
            source.WriteLine($"PolymorphicInfo = new {string.Format(ProtoPolymorphicObjectInfoTypeRef, polymorphicInfo.PolymorphicKeyType.GetFullName())}()");
            source.WriteLine('{');
            source.Indentation++;
            
            source.WriteLine($"PolymorphicIndicateIndex = {polymorphicInfo.PolymorphicIndicateIndex},");
            source.WriteLine($"PolymorphicFallbackToBaseType = {parser.IgnoreDefaultFields.ToString().ToLower()},");
            source.WriteLine(
                $"PolymorphicDerivedTypes = new global::System.Collections.Generic.Dictionary<{polymorphicInfo.PolymorphicKeyType.GetFullName()}, global::System.Type>()");
            source.WriteLine('{');
            source.Indentation++;
            
            foreach (var derivedTypeInfo in polymorphicInfo.PolymorphicTypes)
            {
                source.WriteLine($"[{derivedTypeInfo.Key.ToCSharpString()}] = typeof({derivedTypeInfo.DerivedType.GetFullName()}),");
            }
            
            source.Indentation--;
            source.WriteLine("}");
            source.Indentation--;
            source.WriteLine("},");
        }

        private void EmitPolymorphicDerivedTypeDescriptor(SourceWriter source, PolymorphicTypeInfo polymorphicInfo)
        {
            source.WriteLine("#pragma warning disable CS0108");
            source.WriteLine(
                $"public static {string.Format(ProtoPolymorphicDerivedTypeDescriptorTypeRef,_fullQualifiedName)}? GetPolymorphicTypeDescriptor<TKey>(TKey discriminator)");
            source.WriteLine('{');
            source.Indentation++;
            if (polymorphicInfo.PolymorphicIndicateIndex > 0)
            {
                source.WriteLine("switch (discriminator)");
                source.WriteLine('{');
                source.Indentation++;

                foreach (var derivedTypeInfo in polymorphicInfo.PolymorphicTypes)
                {
                    source.WriteLine($"case {derivedTypeInfo.Key.ToCSharpString()}: return new {string.Format(ProtoPolymorphicDerivedTypeDescriptorTypeRef, _fullQualifiedName)}()");
                    source.WriteLine('{');
                    source.Indentation++;
                    source.WriteLine($"Fields = {derivedTypeInfo.DerivedType.GetFullName()}.{TypeInfoPropertyName}.Fields,");
                    source.WriteLine($"ObjectCreator = {derivedTypeInfo.DerivedType.GetFullName()}.{TypeInfoPropertyName}.ObjectCreator,");
                    source.WriteLine($"IgnoreDefaultFields = {derivedTypeInfo.DerivedType.GetFullName()}.{TypeInfoPropertyName}.IgnoreDefaultFields");
                    source.Indentation--;
                    source.WriteLine("};");
                }
                
                source.WriteLine("default: return null;");
                source.Indentation--;
                source.WriteLine("}");
            }
            else
            {
                source.WriteLine("return null;");
            }

            source.Indentation--;
            source.WriteLine('}');
            source.WriteLine("#pragma warning restore CS0108");
            source.WriteLine();
        }
    }
}