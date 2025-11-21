using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Lagrange.Proto.Generator.Entity;
using Lagrange.Proto.Generator.Utility;
using Lagrange.Proto.Generator.Utility.Extension;
using Lagrange.Proto.Serialization;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Lagrange.Proto.Generator.DiagnosticDescriptors;

namespace Lagrange.Proto.Generator;

public partial class ProtoSourceGenerator
{
    private const string ProtoPackableAttributeFullName = "Lagrange.Proto.ProtoPackableAttribute";
    private const string ProtoValueMemberAttributeFullName = "Lagrange.Proto.ProtoValueMemberAttribute";
    
    private class Parser(ClassDeclarationSyntax context, SemanticModel model)
    {
        public SemanticModel Model { get; } = model;
        
        public List<Diagnostic> Diagnostics { get; } = [];
        
        public string? Namespace { get; private set; }
        
        public List<string> TypeDeclarations { get; } = [];
        
        public bool IgnoreDefaultFields { get; private set; }
        
        public Dictionary<int, ProtoFieldInfo> Fields { get; } = new();

        public PolymorphicTypeInfo PolymorphicInfo { get; } = new();
        
        public BaseTypeInfo? BaseTypeInfo { get; private set; } = null;
        
        public void Parse(CancellationToken token = default)
        {
            Namespace = context.GetNamespace()?.ToString();
            string identifier = context.Identifier.Text;

            if (ModelExtensions.GetDeclaredSymbol(Model, context) is not INamedTypeSymbol classSymbol)
            {
                ReportDiagnostics(UnableToGetSymbol, context.GetLocation(), context.Identifier.Text);
                return;
            }

            if (!classSymbol.Constructors.Any(x => x is { Parameters.Length: 0, DeclaredAccessibility: Accessibility.Public }))
            {
                ReportDiagnostics(MustContainParameterlessConstructor, context.GetLocation(), context.Identifier.Text);
                return;
            }
            
            foreach (var attribute in classSymbol.GetAttributes())
            {
                switch (attribute.AttributeClass?.Name)
                {
                    case "ProtoPackableAttribute":
                        foreach (var argument in attribute.NamedArguments)
                        {
                            switch (argument.Key)
                            {
                                case "IgnoreDefaultFields":
                                    IgnoreDefaultFields = (bool)(argument.Value.Value ?? false);
                                    break;
                            }
                        }
                        break;
                }
            }
            
            if (!TryGetNestedTypeDeclarations(context, Model, token, out var typeDeclarations))
            {
                ReportDiagnostics(MustBePartialClass, context.GetLocation(), context.Identifier.Text);
                return;
            }
            TypeDeclarations.AddRange(typeDeclarations);
            
            PopulateFieldInfo(classSymbol, Fields, identifier, token);
            PopulatePolymorphicInfo(classSymbol, PolymorphicInfo, token);
            
            // Handling BaseType
            var checkingBaseTypeSymbol = classSymbol;
            while (checkingBaseTypeSymbol?.BaseType is not null)
            {
                token.ThrowIfCancellationRequested();
                if (!checkingBaseTypeSymbol.BaseType.GetAttributes()
                        .Any(t => t.AttributeClass?.Name == "ProtoPackableAttribute"))
                {
                    break;
                }
                
                checkingBaseTypeSymbol = checkingBaseTypeSymbol.BaseType;
                PopulateFieldInfo(checkingBaseTypeSymbol, Fields, identifier, token);
            }

            if (checkingBaseTypeSymbol is not null)
            {
                BaseTypeInfo = new BaseTypeInfo();
                BaseTypeInfo.BaseType = checkingBaseTypeSymbol;
                foreach (var attribute in checkingBaseTypeSymbol.GetAttributes())
                {
                    switch (attribute.AttributeClass?.Name)
                    {
                        case "ProtoPackableAttribute":
                            foreach (var argument in attribute.NamedArguments)
                            {
                                switch (argument.Key)
                                {
                                    case "IgnoreDefaultFields":
                                        BaseTypeInfo.IgnoreDefaultFields = (bool)(argument.Value.Value ?? false);
                                        break;
                                }
                            }
                            break;
                    }
                }
                PopulateFieldInfo(BaseTypeInfo.BaseType, BaseTypeInfo.Fields, BaseTypeInfo.BaseType.GetFullName(), token);
                PopulatePolymorphicInfo(BaseTypeInfo.BaseType, BaseTypeInfo.PolymorphicInfo, token);
            }
        }

        private void PopulatePolymorphicInfo(INamedTypeSymbol classSymbol, PolymorphicTypeInfo polymorphicInfo, CancellationToken cancellationToken = default)
        {
            foreach (var attribute in classSymbol.GetAttributes())
            {
                cancellationToken.ThrowIfCancellationRequested();
                switch (attribute.AttributeClass?.Name)
                {
                    case "ProtoDerivedTypeAttribute":
                        if (polymorphicInfo.PolymorphicIndicateIndex == 0)
                            polymorphicInfo.PolymorphicIndicateIndex = 1; // set to default
                        // get key type
                        if (attribute.AttributeClass.TypeArguments.First() is not INamedTypeSymbol keyType)
                        {
                            ReportDiagnostics(UnableToGetSymbol, context.GetLocation(),
                                attribute.AttributeClass.TypeArguments.First().ToDisplayString());
                            return;
                        }

                        polymorphicInfo.PolymorphicKeyType = keyType;

                        // get derived type, in typeof
                        var derivedTypeConstant = attribute.ConstructorArguments.First();
                        if (derivedTypeConstant.Kind != TypedConstantKind.Type)
                        {
                            ReportDiagnostics(UnableToGetSymbol, context.GetLocation(),
                                derivedTypeConstant.ToCSharpString());
                            return;
                        }

                        if (derivedTypeConstant.Value is not INamedTypeSymbol derivedType)
                        {
                            ReportDiagnostics(UnableToGetSymbol, context.GetLocation(),
                                derivedTypeConstant.ToCSharpString());
                            return;
                        }

                        // get type discriminator
                        var typeDiscriminatorConstant = attribute.ConstructorArguments.ElementAtOrDefault(1);
                        polymorphicInfo.PolymorphicTypes.Add(new PolymorphicDerivedTypeInfo
                        {
                            DerivedType = derivedType, Key = typeDiscriminatorConstant
                        });
                        break;
                    case "ProtoPolymorphicAttribute":
                        foreach (var argument in attribute.NamedArguments)
                        {
                            switch (argument.Key)
                            {
                                case "FieldNumber":
                                    polymorphicInfo.PolymorphicIndicateIndex = (uint)(argument.Value.Value ?? 0);
                                    break;
                                case "FallbackToBaseType":
                                    polymorphicInfo.PolymorphicFallbackToBaseType =
                                        (bool)(argument.Value.Value ?? true);
                                    break;
                            }
                        }

                        break;
                }
            }
            
        }
        
        private void PopulateFieldInfo(INamedTypeSymbol classSymbol, Dictionary<int, ProtoFieldInfo> fields, string identifier = "" ,CancellationToken token = default)
        {
            var members = classSymbol.GetMembers()
                .Where(x => x is IPropertySymbol or IFieldSymbol)
                .Where(x => x.GetAttributes().Any(t=>t.AttributeClass?.Name == "ProtoMemberAttribute"));
            
            foreach (var symbol in members)
            {
                token.ThrowIfCancellationRequested();
                
                var member = symbol.DeclaringSyntaxReferences.FirstOrDefault()!.GetSyntax(token);
                if (symbol.IsStatic)
                {
                    ReportDiagnostics(MustNotBeStatic, member.GetLocation(), symbol.Name, identifier);
                    continue;
                }
                
                var attribute = symbol.GetAttributes().First(x => x.AttributeClass?.Name == "ProtoMemberAttribute");
                int field = (int)(attribute.ConstructorArguments[0].Value ?? throw new InvalidOperationException("Unable to get field number."));
                if (fields.ContainsKey(field))
                {
                    ReportDiagnostics(DuplicateFieldNumber, member.GetLocation(), field, identifier);
                    continue;
                }
                
                var typeSymbol = symbol switch
                {
                    IPropertySymbol propertySymbol => propertySymbol.Type,
                    IFieldSymbol fieldSymbol => fieldSymbol.Type,
                    _ => throw new InvalidOperationException("Unsupported member type.")
                };
                var wireType = ProtoHelper.GetWireType(typeSymbol);
                bool signed = false;

                if (wireType == WireType.LengthDelimited && typeSymbol.IsUserDefinedType())
                {
                    var typeAttribute = typeSymbol.GetAttributes().FirstOrDefault(x => x.AttributeClass?.ToDisplayString() == ProtoPackableAttributeFullName);
                    if (typeAttribute == null && !SymbolResolver.IsNodesType(typeSymbol))
                    { 
                        ReportDiagnostics(NestedTypeMustBeProtoPackable, member.GetLocation(), typeSymbol.Name, identifier);
                        continue;
                    }
                }

                if (SymbolResolver.IsMapType(typeSymbol, out var keyType, out var valueType))
                {
                    var keyWireType = ProtoHelper.GetWireType(keyType);
                    var valueWireType = ProtoHelper.GetWireType(valueType);
                    bool valueSigned = false;

                    ReadProtoMemberAttribute(attribute, typeSymbol, ref keyWireType, member, field, identifier, ref signed);
                    var valueAttribute = symbol.GetAttributes().FirstOrDefault(x => x.AttributeClass?.ToDisplayString() == ProtoValueMemberAttributeFullName);
                    if (valueAttribute != null) ReadProtoMemberAttribute(valueAttribute, typeSymbol, ref valueWireType, member, field, identifier, ref valueSigned);

                    fields[field] = new ProtoFieldInfo(symbol, typeSymbol, wireType, signed) 
                    { 
                        ExtraTypeInfo =
                        {
                            new ProtoTypeInfo(keyType, keyWireType, signed),
                            new ProtoTypeInfo(valueType, valueWireType, valueSigned)
                        } 
                    };
                }
                else
                {
                    ReadProtoMemberAttribute(attribute, typeSymbol, ref wireType, member, field, identifier, ref signed);
                    fields[field] = new ProtoFieldInfo(symbol, typeSymbol, wireType, signed);
                }
            }
        }
        
        private void ReadProtoMemberAttribute(AttributeData attribute, ITypeSymbol typeSymbol, ref WireType wireType, SyntaxNode member, int field, string identifier, ref bool signed)
        {
            foreach (var argument in attribute.NamedArguments)
            {
                switch (argument.Key)
                {
                    case "NumberHandling":
                    {
                        if (wireType != WireType.VarInt)
                        {
                            ReportDiagnostics(InvalidNumberHandling, member.GetLocation(), field, identifier);
                            continue;
                        }
                            
                        var value = (ProtoNumberHandling)(argument.Value.Value ?? throw new InvalidOperationException("Unable to get number handling."));
                        if (value.HasFlag(ProtoNumberHandling.Signed)) signed = true;
                        if (value.HasFlag(ProtoNumberHandling.Fixed32)) wireType = WireType.Fixed32;
                        if (value.HasFlag(ProtoNumberHandling.Fixed64)) wireType = WireType.Fixed64;
                        break;
                    }
                }
            }
            
            if (SymbolResolver.IsDynamicNodesType(typeSymbol))
            {
                var nodesWireType = WireType.Unknown;
                    
                foreach (var kv in attribute.NamedArguments)
                {
                    switch (kv.Key)
                    {
                        case "NodesWireType":
                            nodesWireType = (WireType)(kv.Value.Value ?? throw new InvalidOperationException("Unable to get wire type."));
                            break;
                    }
                }
                    
                if (nodesWireType == WireType.Unknown)
                {
                    ReportDiagnostics(InvalidNodesWireType, member.GetLocation(), field, identifier);
                    return;
                }
                wireType = nodesWireType;
            }
        }

        private static bool TryGetNestedTypeDeclarations(ClassDeclarationSyntax contextClassSyntax, SemanticModel semanticModel, CancellationToken cancellationToken, [NotNullWhen(true)] out List<string>? typeDeclarations)
        {
            typeDeclarations = null;

            for (TypeDeclarationSyntax? currentType = contextClassSyntax; currentType != null; currentType = currentType.Parent as TypeDeclarationSyntax)
            {
                var stringBuilder = new StringBuilder();
                bool isPartialType = false;

                foreach (var modifier in currentType.Modifiers)
                {
                    stringBuilder.Append(modifier.Text);
                    stringBuilder.Append(' ');
                    isPartialType |= modifier.IsKind(SyntaxKind.PartialKeyword);
                }

                if (!isPartialType)
                {
                    typeDeclarations = null;
                    return false;
                }

                stringBuilder.Append(currentType.GetTypeKindKeyword());
                stringBuilder.Append(' ');

                var typeSymbol = semanticModel.GetDeclaredSymbol(currentType, cancellationToken);
                if (typeSymbol == null)
                {
                    typeDeclarations = null;
                    return false;
                }

                string typeName = typeSymbol.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);
                stringBuilder.Append(typeName);

                (typeDeclarations ??= []).Add(stringBuilder.ToString());
            }

            typeDeclarations ??= [];
            return true;
        }
        
        private void ReportDiagnostics(DiagnosticDescriptor descriptor, Location location, params object[] args)
        {
            var diagnostic = Diagnostic.Create(descriptor, location, args);
            Diagnostics.Add(diagnostic);
        }
    }
}