using Lagrange.Proto.Generator.Entity;
using Lagrange.Proto.Generator.Utility;
using Lagrange.Proto.Generator.Utility.Extension;
using Lagrange.Proto.Serialization;
using Microsoft.CodeAnalysis;

namespace Lagrange.Proto.Generator;

public partial class ProtoSourceGenerator
{
    private partial class Emitter
    {
        private const string ProtoWriterTypeRef = "global::Lagrange.Proto.Primitives.ProtoWriter";

        private const string WriterVarName = "writer";
        private const string ObjectVarName = "obj";
        private const string ValueVarName = "value";
        
        private const string WriteRawByteMethodName = "WriteRawByte";
        private const string EncodeVarIntMethodName = "EncodeVarInt";
        private const string EncodeFixed32MethodName = "EncodeFixed32";
        private const string EncodeFixed64MethodName = "EncodeFixed64";
        private const string ZigZagEncodeMethodName = "ZigZagEncode";

        private const string EncodeStringMethodName = "EncodeString";
        private const string EncodeBytesMethodName = "EncodeBytes";
        private const string EncodeResolvableMethodName = "EncodeResolvable";
        

        private void EmitSerializeMethod(SourceWriter source)
        {
            source.WriteLine($"public static void SerializeHandler({_fullQualifiedName} {ObjectVarName}, {ProtoWriterTypeRef} {WriterVarName})");
            source.WriteLine("{");
            source.Indentation++;
            
            if (parser.BaseTypeSymbol is not null)
            {
                source.WriteLine($"{parser.BaseTypeSymbol.GetFullName()}.SerializeHandler({ObjectVarName},{WriterVarName});");
            }
            
            source.WriteLine($"SerializeHandlerCore({ObjectVarName}, {WriterVarName});");
            source.Indentation--;
            source.WriteLine("}");
            source.WriteLine();
            
            source.WriteLine($"private static void SerializeHandlerCore({_fullQualifiedName} {ObjectVarName}, {ProtoWriterTypeRef} {WriterVarName})");
            source.WriteLine("{");
            source.Indentation++;
            if (parser.PolymorphicInfo.PolymorphicIndicateIndex > 0)
            {
                // write indicate field first
                var idx = (int)parser.PolymorphicInfo.PolymorphicIndicateIndex;
                var indicatorField = parser.Fields[idx];
                EmitMembers(source, idx, indicatorField);
                source.WriteLine();
                
                source.WriteLine($"switch ({ObjectVarName})");
                source.WriteLine("{");
                source.Indentation++;
                for (var index = 0; index < parser.PolymorphicInfo.PolymorphicTypes.Count; index++)
                {
                    var kv = parser.PolymorphicInfo.PolymorphicTypes[index];
                    source.WriteLine($"case {kv.DerivedType.GetFullName()} derived{index}:");
                    source.Indentation++;
                    source.WriteLine($"{kv.DerivedType.GetFullName()}.SerializeHandlerCore(derived{index}, {WriterVarName});");
                    source.WriteLine("break;");
                    source.Indentation--;
                }
                source.Indentation--;
                source.WriteLine("}");
                source.WriteLine();
            }
            foreach (var kv in parser.Fields)
            {
                int field = kv.Key;
                if (parser.PolymorphicInfo.PolymorphicIndicateIndex == field) continue; // already written
                var info = kv.Value;
                
                EmitMembers(source, field, info);
                source.WriteLine();
            }
            
            
            source.Indentation--;
            source.WriteLine("}");
        }

        private void EmitMembers(SourceWriter source, int field, ProtoFieldInfo info)
        {
            uint tag = (uint)field << 3 | (byte)info.WireType;
            var encodedTag = ProtoHelper.EncodeVarInt(tag);
            
            string memberName =  info.TypeSymbol.IsValueType && info.TypeSymbol.IsNullable() 
                ? $"{ObjectVarName}.{info.Symbol.Name}.Value"
                : $"{ObjectVarName}.{info.Symbol.Name}";
            if (parser.IgnoreDefaultFields)
            {
                if (info.TypeSymbol.IsValueType) // check with default
                {
                    EmitIfNotDefaultStatement(source, $"{ObjectVarName}.{info.Symbol.Name}", writer =>
                    {
                        EmitRawTags(writer, encodedTag);
                        EmitMember(writer, field, info, memberName);
                    });
                }
                else
                {
                    EmitIfShouldSerializeStatement(source, tag, writer =>
                    {
                        EmitRawTags(writer, encodedTag);
                        EmitMember(writer, field, info, memberName);
                    });
                }
            }
            else
            {
                if (info.TypeSymbol.IsValueType)
                {
                    if (info.TypeSymbol.IsNullable()) // write xxxx.Value
                    {
                        EmitIfNotNullStatement(source, $"{ObjectVarName}.{info.Symbol.Name}", writer =>
                        {
                            EmitRawTags(writer, encodedTag);
                            EmitMember(writer, field, info, memberName);
                        });
                    }
                    else // write directly
                    {
                        EmitRawTags(source, encodedTag);
                        EmitMember(source, field, info, memberName);
                    }
                }
                else
                {
                    EmitIfShouldSerializeStatement(source, tag, writer =>
                    {
                        EmitRawTags(writer, encodedTag);
                        EmitMember(writer, field, info, memberName);
                    });
                }
            }
        }
        
        private static void EmitRawTags(SourceWriter source, byte[] tag)
        {
            foreach (byte i in tag) source.WriteLine($"{WriterVarName}.{WriteRawByteMethodName}({i});");
        }

        private void EmitMember(SourceWriter source, int field, ProtoFieldInfo info, string memberName)
        {
            if (SymbolResolver.IsMapType(info.TypeSymbol, out _, out _))
            { 
                source.WriteLine($"{TypeInfoPropertyName}.Fields[{field << 3 | (byte)info.WireType}].Write({WriterVarName}, {ObjectVarName});");
                return;
            }
            
            if (!SymbolResolver.IsRepeatedType(info.TypeSymbol, out _))
            {
                string? special = info.WireType switch
                {
                    WireType.VarInt when info.TypeSymbol.IsIntegerType() && info.IsSigned => GetZigZagEncodeCall(info.TypeSymbol, memberName),
                    WireType.VarInt when info.TypeSymbol.IsIntegerType() => $"{WriterVarName}.{EncodeVarIntMethodName}({memberName});",
                    WireType.Fixed32 when info.IsSigned => GetZigZagEncodeFixed32Call(info.TypeSymbol, memberName),
                    WireType.Fixed64 when info.IsSigned => GetZigZagEncodeFixed64Call(info.TypeSymbol, memberName),
                    WireType.Fixed32 => $"{WriterVarName}.{EncodeFixed32MethodName}({memberName});",
                    WireType.Fixed64 => $"{WriterVarName}.{EncodeFixed64MethodName}({memberName});",
                    WireType.LengthDelimited when info.TypeSymbol.SpecialType == SpecialType.System_String => $"{WriterVarName}.{EncodeStringMethodName}({memberName});",
                    WireType.LengthDelimited when info.TypeSymbol is IArrayTypeSymbol { ElementType.SpecialType: SpecialType.System_Byte } => $"{WriterVarName}.{EncodeBytesMethodName}({memberName});",
                    _ => null
                };

                if (special != null)
                {
                    source.WriteLine(special);
                    return;
                }
            }
            
            source.WriteLine(info.IsSigned
                ? $"{WriterVarName}.{EncodeResolvableMethodName}({field}, {WireTypeTypeRef}.{info.WireType}, {memberName}, {ProtoNumberHandlingTypeRef}.{(info.IsSigned ? "Signed" : "Default")});"
                : $"{WriterVarName}.{EncodeResolvableMethodName}({field}, {WireTypeTypeRef}.{info.WireType}, {memberName});");
        }
        
        private static void EmitIfNotNullStatement(SourceWriter source, string variableName, Action<SourceWriter> emitAction)
        {
            source.WriteLine($"if ({variableName} != null)");
            source.WriteLine("{");
            source.Indentation++;
            emitAction(source);
            source.Indentation--;
            source.WriteLine("}");
        }
        
        private static void EmitIfNotDefaultStatement(SourceWriter source, string variableName, Action<SourceWriter> emitAction)
        {
            source.WriteLine($"if ({variableName} != default)");
            source.WriteLine("{");
            source.Indentation++;
            emitAction(source);
            source.Indentation--;
            source.WriteLine("}");
        }
        
        private void EmitIfShouldSerializeStatement(SourceWriter source, uint tag, Action<SourceWriter> emitAction)
        {
            source.WriteLine($"if ({_fullQualifiedName}.{TypeInfoPropertyName}.Fields[{tag}].{ShouldSerializeTypeRef}({ObjectVarName}, {parser.IgnoreDefaultFields.ToString().ToLower()}))");
            source.WriteLine("{");
            source.Indentation++;
            emitAction(source);
            source.Indentation--;
            source.WriteLine("}");
        }
        
        private string GetZigZagEncodeCall(ITypeSymbol typeSymbol, string memberName)
        {
            // For signed byte types, we need to cast the ZigZag encoded result to unsigned
            // to prevent sign extension when writing as varint
            return typeSymbol.SpecialType switch
            {
                SpecialType.System_SByte => $"{WriterVarName}.{EncodeVarIntMethodName}((byte){ProtoHelperTypeRef}.{ZigZagEncodeMethodName}({memberName}));",
                SpecialType.System_Int16 => $"{WriterVarName}.{EncodeVarIntMethodName}((ushort){ProtoHelperTypeRef}.{ZigZagEncodeMethodName}({memberName}));",
                SpecialType.System_Int32 => $"{WriterVarName}.{EncodeVarIntMethodName}((uint){ProtoHelperTypeRef}.{ZigZagEncodeMethodName}({memberName}));",
                SpecialType.System_Int64 => $"{WriterVarName}.{EncodeVarIntMethodName}((ulong){ProtoHelperTypeRef}.{ZigZagEncodeMethodName}({memberName}));",
                _ => $"{WriterVarName}.{EncodeVarIntMethodName}({ProtoHelperTypeRef}.{ZigZagEncodeMethodName}({memberName}));"
            };
        }
        
        private string GetZigZagEncodeFixed32Call(ITypeSymbol typeSymbol, string memberName)
        {
            return typeSymbol.SpecialType switch
            {
                SpecialType.System_SByte => $"{WriterVarName}.{EncodeFixed32MethodName}((byte){ProtoHelperTypeRef}.{ZigZagEncodeMethodName}({memberName}));",
                SpecialType.System_Int16 => $"{WriterVarName}.{EncodeFixed32MethodName}((ushort){ProtoHelperTypeRef}.{ZigZagEncodeMethodName}({memberName}));",
                SpecialType.System_Int32 => $"{WriterVarName}.{EncodeFixed32MethodName}((uint){ProtoHelperTypeRef}.{ZigZagEncodeMethodName}({memberName}));",
                _ => $"{WriterVarName}.{EncodeFixed32MethodName}({ProtoHelperTypeRef}.{ZigZagEncodeMethodName}({memberName}));"
            };
        }
        
        private string GetZigZagEncodeFixed64Call(ITypeSymbol typeSymbol, string memberName)
        {
            return typeSymbol.SpecialType switch
            {
                SpecialType.System_Int64 => $"{WriterVarName}.{EncodeFixed64MethodName}((ulong){ProtoHelperTypeRef}.{ZigZagEncodeMethodName}({memberName}));",
                _ => $"{WriterVarName}.{EncodeFixed64MethodName}({ProtoHelperTypeRef}.{ZigZagEncodeMethodName}({memberName}));"
            };
        }
    }
}