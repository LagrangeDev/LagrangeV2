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
        private const string ProtoReaderTypeRef = "global::Lagrange.Proto.Primitives.ProtoReader";
        private const string ProtoSerializerTypeRef = "global::Lagrange.Proto.Serialization.ProtoSerializer";
        private const string EncodingTypeRef = "global::System.Text.Encoding";

        private const string ReaderVarName = "reader";
        private const string TargetVarName = "target";
        private const string TagVarName = "tag";

        private const string DecodeVarIntMethodName = "DecodeVarInt";
        private const string DecodeVarIntUnsafeMethodName = "DecodeVarIntUnsafe";
        private const string DecodeFixed32MethodName = "DecodeFixed32";
        private const string DecodeFixed64MethodName = "DecodeFixed64";
        private const string CreateSpanMethodName = "CreateSpan";
        private const string SkipFieldMethodName = "SkipField";
        private const string ZigZagDecodeMethodName = "ZigZagDecode";

        private void EmitDeserializeMethod(SourceWriter source)
        {
            source.WriteLine($"public static {_fullQualifiedName} DeserializeHandler(ref {ProtoReaderTypeRef} {ReaderVarName})");
            source.WriteLine("{");
            source.Indentation++;

            source.WriteLine($"var {TargetVarName} = new {_fullQualifiedName}();");
            source.WriteLine();

            source.WriteLine($"while (!{ReaderVarName}.IsCompleted)");
            source.WriteLine("{");
            source.Indentation++;

            source.WriteLine($"uint {TagVarName} = {ReaderVarName}.{DecodeVarIntUnsafeMethodName}<uint>();");

            // Use switch expression for field dispatch
            source.WriteLine($"switch ({TagVarName})");
            source.WriteLine("{");
            source.Indentation++;

            foreach (var kv in parser.Fields)
            {
                int field = kv.Key;
                var info = kv.Value;

                EmitDeserializeCaseStatement(source, field, info);
            }

            // Default case: skip unknown fields
            source.WriteLine("default:");
            source.Indentation++;
            source.WriteLine($"{ReaderVarName}.{SkipFieldMethodName}(({WireTypeTypeRef})({TagVarName} & 0x07));");
            source.WriteLine("break;");
            source.Indentation--;

            source.Indentation--;
            source.WriteLine("}"); // end switch

            source.Indentation--;
            source.WriteLine("}"); // end while

            source.WriteLine();
            source.WriteLine($"return {TargetVarName};");

            source.Indentation--;
            source.WriteLine("}");
        }

        private void EmitDeserializeCaseStatement(SourceWriter source, int field, ProtoFieldInfo info)
        {
            uint tag = (uint)field << 3 | (byte)info.WireType;

            source.WriteLine($"case {tag}:");
            source.Indentation++;

            string memberName = $"{TargetVarName}.{info.Symbol.Name}";

            // Handle map types via TypeInfo (complex logic, delegate to existing converter)
            if (SymbolResolver.IsMapType(info.TypeSymbol, out _, out _))
            {
                source.WriteLine($"{TypeInfoPropertyName}.Fields[{tag}].Read(ref {ReaderVarName}, {TargetVarName});");
                source.WriteLine("break;");
                source.Indentation--;
                return;
            }

            // Handle repeated types via TypeInfo (complex logic with state)
            if (SymbolResolver.IsRepeatedType(info.TypeSymbol, out _))
            {
                source.WriteLine($"{TypeInfoPropertyName}.Fields[{tag}].Read(ref {ReaderVarName}, {TargetVarName});");
                source.WriteLine("break;");
                source.Indentation--;
                return;
            }

            // Handle byte[] specially (requires multi-line code)
            if (info.TypeSymbol is IArrayTypeSymbol { ElementType.SpecialType: SpecialType.System_Byte })
            {
                source.WriteLine("{");
                source.Indentation++;
                source.WriteLine($"int len = {ReaderVarName}.{DecodeVarIntMethodName}<int>();");
                source.WriteLine($"if (len == 0) {memberName} = global::System.Array.Empty<byte>();");
                source.WriteLine("else");
                source.WriteLine("{");
                source.Indentation++;
                source.WriteLine($"var buf = global::System.GC.AllocateUninitializedArray<byte>(len);");
                source.WriteLine($"{ReaderVarName}.{CreateSpanMethodName}(len).CopyTo(buf);");
                source.WriteLine($"{memberName} = buf;");
                source.Indentation--;
                source.WriteLine("}");
                source.Indentation--;
                source.WriteLine("}");
                source.WriteLine("break;");
                source.Indentation--;
                return;
            }

            // Generate inline read for primitive and simple types
            string? readExpression = GenerateReadExpression(field, info);

            if (readExpression != null)
            {
                source.WriteLine($"{memberName} = {readExpression};");
            }
            else
            {
                // Fallback to TypeInfo-based read for complex types
                source.WriteLine($"{TypeInfoPropertyName}.Fields[{tag}].Read(ref {ReaderVarName}, {TargetVarName});");
            }

            source.WriteLine("break;");
            source.Indentation--;
        }

        private string? GenerateReadExpression(int field, ProtoFieldInfo info)
        {
            // Handle nullable wrapper
            if (info.TypeSymbol.IsValueType && info.TypeSymbol.IsNullable())
            {
                var underlyingType = SymbolResolver.GetGenericTypeNonNull(info.TypeSymbol);
                var underlyingInfo = new ProtoFieldInfo(info.Symbol, underlyingType, info.WireType, info.IsSigned);
                string? innerRead = GenerateReadExpressionCore(field, underlyingInfo);
                return innerRead; // Nullable<T> can be assigned from T directly
            }

            return GenerateReadExpressionCore(field, info);
        }

        private string? GenerateReadExpressionCore(int field, ProtoFieldInfo info)
        {
            var typeSymbol = info.TypeSymbol;

            // VarInt types (integers, booleans, enums)
            if (info.WireType == WireType.VarInt)
            {
                if (typeSymbol.SpecialType == SpecialType.System_Boolean)
                {
                    return $"{ReaderVarName}.{DecodeVarIntMethodName}<byte>() != 0";
                }

                if (typeSymbol.TypeKind == TypeKind.Enum)
                {
                    var underlyingType = ((INamedTypeSymbol)typeSymbol).EnumUnderlyingType;
                    string underlyingTypeName = underlyingType?.GetFullName() ?? "int";
                    return $"({typeSymbol.GetFullName()}){ReaderVarName}.{DecodeVarIntMethodName}<{underlyingTypeName}>()";
                }

                if (typeSymbol.IsIntegerType())
                {
                    string typeName = typeSymbol.GetFullName();

                    if (info.IsSigned)
                    {
                        // ZigZag decode for signed types
                        string unsignedType = GetUnsignedType(typeSymbol);
                        return $"{ProtoHelperTypeRef}.{ZigZagDecodeMethodName}<{typeName}>({ReaderVarName}.{DecodeVarIntMethodName}<{unsignedType}>())";
                    }

                    return $"{ReaderVarName}.{DecodeVarIntMethodName}<{typeName}>()";
                }
            }

            // Fixed32 types
            if (info.WireType == WireType.Fixed32)
            {
                string typeName = typeSymbol.GetFullName();

                if (info.IsSigned && typeSymbol.IsIntegerType())
                {
                    string unsignedType = GetUnsignedType(typeSymbol);
                    return $"{ProtoHelperTypeRef}.{ZigZagDecodeMethodName}<{typeName}>({ReaderVarName}.{DecodeFixed32MethodName}<{unsignedType}>())";
                }

                return $"{ReaderVarName}.{DecodeFixed32MethodName}<{typeName}>()";
            }

            // Fixed64 types
            if (info.WireType == WireType.Fixed64)
            {
                string typeName = typeSymbol.GetFullName();

                if (info.IsSigned && typeSymbol.IsIntegerType())
                {
                    return $"{ProtoHelperTypeRef}.{ZigZagDecodeMethodName}<{typeName}>({ReaderVarName}.{DecodeFixed64MethodName}<ulong>())";
                }

                return $"{ReaderVarName}.{DecodeFixed64MethodName}<{typeName}>()";
            }

            // LengthDelimited types
            if (info.WireType == WireType.LengthDelimited)
            {
                // String
                if (typeSymbol.SpecialType == SpecialType.System_String)
                {
                    return $"{EncodingTypeRef}.UTF8.GetString({ReaderVarName}.{CreateSpanMethodName}({ReaderVarName}.{DecodeVarIntMethodName}<int>()))";
                }

                // byte[] - handled specially below
                if (typeSymbol is IArrayTypeSymbol { ElementType.SpecialType: SpecialType.System_Byte })
                {
                    // Return null to use special handling in EmitDeserializeCaseStatement
                    return null;
                }

                // Nested ProtoPackable object
                if (SymbolResolver.IsProtoPackable(typeSymbol))
                {
                    string nestedTypeName = typeSymbol.GetFullName();
                    return $"{ProtoSerializerTypeRef}.DeserializeProtoPackable<{nestedTypeName}>({ReaderVarName}.{CreateSpanMethodName}({ReaderVarName}.{DecodeVarIntMethodName}<int>()))";
                }
            }

            // Unknown type - return null to use fallback
            return null;
        }

        private static string GetUnsignedType(ITypeSymbol typeSymbol)
        {
            return typeSymbol.SpecialType switch
            {
                SpecialType.System_SByte => "byte",
                SpecialType.System_Int16 => "ushort",
                SpecialType.System_Int32 => "uint",
                SpecialType.System_Int64 => "ulong",
                _ => typeSymbol.GetFullName()
            };
        }
    }
}
