using System.CodeDom.Compiler;
using Lagrange.Proto.Generator.Entity;
using Lagrange.Proto.Generator.Utility;
using Lagrange.Proto.Generator.Utility.Extension;
using Lagrange.Proto.Serialization;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SF = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using SK = Microsoft.CodeAnalysis.CSharp.SyntaxKind;

namespace Lagrange.Proto.Generator;

public partial class ProtoSourceGenerator
{ 
    private partial class Emitter
    {
        private MethodDeclarationSyntax EmitSerializeHandlerMethod()
        {
            var syntax = new List<StatementSyntax> { EmitNullableCheckStatement(true, "obj", SF.ReturnStatement()) };
                
            foreach (var kv in parser.Fields)
            {
                var type = kv.Value.TypeSyntax;
                string name = kv.Value.Name;
                var tag = EmitTagSerializeStatement(kv.Key, kv.Value.WireType);
                var field = EmitMemberStatement(kv.Value, kv.Key, name);

                var block = SF.Block(SF.List<StatementSyntax>([..tag, ..field]));

                if (parser.Model.GetTypeSymbol(type).IsValueType && !type.IsNullableType())
                {
                    if (parser.IgnoreDefaultFields)
                    {
                        syntax.Add(EmitDefaultCheckStatement(false, $"obj.{name}", block, false));
                    }
                    else
                    {
                        syntax.AddRange(tag);
                        syntax.AddRange(field);
                        syntax[syntax.Count - 1] = syntax[syntax.Count - 1].WithTrailingTrivia(SF.Comment("\n"));
                    }
                }
                else
                {
                    syntax.Add(EmitNullableCheckStatement(false, $"obj.{name}", block, false));
                }
            }

            string classFullName = $"global::{parser.Namespace}.{parser.Identifier}?";
            var parameters = SF.ParameterList()
                .AddParameters(SF.Parameter(SF.Identifier("obj")).WithType(SF.ParseTypeName(classFullName)))
                .AddParameters(SF.Parameter(SF.Identifier("writer")).WithType(SF.ParseTypeName(WriterFullName)));

            return SF.MethodDeclaration(SF.PredefinedType(SF.Token(SK.VoidKeyword)), "SerializeHandler")
                .AddModifiers(SF.Token(SK.PublicKeyword), SF.Token(SK.StaticKeyword))
                .WithParameterList(parameters)
                .WithBody(SF.Block(SF.List(syntax)));
        }
            
        private StatementSyntax[] EmitMemberStatement(ProtoFieldInfo fieldInfo, int field, string identifier)
        {
            var type = fieldInfo.TypeSyntax;
            var symbol = parser.Model.GetTypeSymbol(type);
            if (type.IsNullableType() && symbol.IsValueType) identifier += ".Value";

            return fieldInfo.WireType switch
            {
                WireType.VarInt when symbol.IsEnumType() => [EmitResolvableSerializeStatement(identifier, field, fieldInfo.WireType)],
                WireType.VarInt when symbol.SpecialType == SpecialType.System_Boolean => [EmitBooleanSerializeStatement(identifier)],
                WireType.VarInt => [EmitVarIntSerializeStatement(identifier, fieldInfo.IsSigned)],
                WireType.Fixed32 => [EmitFixed32SerializeStatement(identifier, fieldInfo.IsSigned)],
                WireType.Fixed64 => [EmitFixed64SerializeStatement(identifier, fieldInfo.IsSigned)],
                WireType.LengthDelimited when type.IsStringType() => [EmitStringSerializeStatement(identifier)],
                WireType.LengthDelimited when type.IsByteArrayType() => [EmitBytesSerializeStatement(identifier)],
                WireType.LengthDelimited when symbol.IsUserDefinedType() => EmitProtoPackableSerializeStatement(type.ToString(), identifier),
                _ => [EmitResolvableSerializeStatement(identifier, field, fieldInfo.WireType)]
            };
        }

        private static ExpressionStatementSyntax[] EmitTagSerializeStatement(int field, WireType wireType)
        {
            int tag = (field << 3) | (byte)wireType;
            var encoded = ProtoHelper.EncodeVarInt(tag);
            string comment = $"// Field {field} | WireType {wireType} | Tag {tag} = {field} | {(byte)wireType} ({wireType}) << 3";

            var access = SF.MemberAccessExpression(SK.SimpleMemberAccessExpression, SF.IdentifierName("writer"), SF.IdentifierName("WriteRawByte"));

            return encoded.Select(num => SF.LiteralExpression(SK.NumericLiteralExpression, SF.Literal(num)))
                .Select(les =>
                    SF.ExpressionStatement(SF.InvocationExpression(access).AddArgumentListArguments(SF.Argument(les)))
                        .WithTrailingTrivia(SF.Comment(comment)))
                .ToArray();
        }
            
        private static StatementSyntax EmitVarIntSerializeStatement(string name, bool isSigned)
        {
            ExpressionSyntax arg = SF.MemberAccessExpression(SK.SimpleMemberAccessExpression, SF.IdentifierName("obj"), SF.IdentifierName(name));
            var access = SF.MemberAccessExpression(SK.SimpleMemberAccessExpression, SF.IdentifierName("writer"), SF.IdentifierName("EncodeVarInt"));

            if (isSigned)
            {
                var zigZag = SF.MemberAccessExpression(SK.SimpleMemberAccessExpression, SF.IdentifierName("global::Lagrange.Proto.Utility.ProtoHelper"), SF.IdentifierName("ZigZagEncode"));
                arg = SF.InvocationExpression(zigZag).AddArgumentListArguments(SF.Argument(arg));
            }
                
            return SF.ExpressionStatement(SF.InvocationExpression(access).AddArgumentListArguments(SF.Argument(arg)));
        }
        
        private static StatementSyntax EmitBooleanSerializeStatement(string name)
        {
            var comparision = SF.ParenthesizedExpression(SF.ConditionalExpression(
                SF.MemberAccessExpression(SK.SimpleMemberAccessExpression, SF.IdentifierName("obj"), SF.IdentifierName(name)),
                SF.LiteralExpression(SK.NumericLiteralExpression, SF.Literal(1)),
                SF.LiteralExpression(SK.NumericLiteralExpression, SF.Literal(0))));
            var access = SF.MemberAccessExpression(SK.SimpleMemberAccessExpression, SF.IdentifierName("writer"), SF.IdentifierName("WriteRawByte"));
            var cast = SF.CastExpression(SF.PredefinedType(SF.Token(SK.ByteKeyword)), comparision);
            return SF.ExpressionStatement(SF.InvocationExpression(access).AddArgumentListArguments(SF.Argument(cast)));
        }

        
        private static StatementSyntax EmitResolvableSerializeStatement(string name, int field, WireType wireType)
        {
            var tagArg = SF.LiteralExpression(SK.NumericLiteralExpression, SF.Literal(field));
            var wireTypeArg = SF.MemberAccessExpression(SK.SimpleMemberAccessExpression, SF.IdentifierName("global::Lagrange.Proto.Serialization.WireType"), SF.IdentifierName(wireType.ToString()));
            var arg = SF.MemberAccessExpression(SK.SimpleMemberAccessExpression, SF.IdentifierName("obj"), SF.IdentifierName(name));
            var access = SF.MemberAccessExpression(SK.SimpleMemberAccessExpression, SF.IdentifierName("writer"), SF.IdentifierName("EncodeResolvable"));
            return SF.ExpressionStatement(SF.InvocationExpression(access).AddArgumentListArguments(SF.Argument(tagArg), SF.Argument(wireTypeArg), SF.Argument(arg)));
        }
            
        private static StatementSyntax EmitFixed32SerializeStatement(string name, bool isSigned)
        {
            ExpressionSyntax arg = SF.MemberAccessExpression(SK.SimpleMemberAccessExpression, SF.IdentifierName("obj"), SF.IdentifierName(name));
            var access = SF.MemberAccessExpression(SK.SimpleMemberAccessExpression, SF.IdentifierName("writer"), SF.IdentifierName("EncodeFixed32"));
                
            if (isSigned)
            {
                var zigZag = SF.MemberAccessExpression(SK.SimpleMemberAccessExpression, SF.IdentifierName("global::Lagrange.Proto.Utility.ProtoHelper"), SF.IdentifierName("ZigZagEncode"));
                arg = SF.InvocationExpression(zigZag).AddArgumentListArguments(SF.Argument(arg));
            }
                
            return SF.ExpressionStatement(SF.InvocationExpression(access).AddArgumentListArguments(SF.Argument(arg)));
        }
            
        private static StatementSyntax EmitFixed64SerializeStatement(string name, bool isSigned)
        {
            ExpressionSyntax arg = SF.MemberAccessExpression(SK.SimpleMemberAccessExpression, SF.IdentifierName("obj"), SF.IdentifierName(name));
            var access = SF.MemberAccessExpression(SK.SimpleMemberAccessExpression, SF.IdentifierName("writer"), SF.IdentifierName("EncodeFixed64"));
                
            if (isSigned)
            {
                var zigZag = SF.MemberAccessExpression(SK.SimpleMemberAccessExpression, SF.IdentifierName("global::Lagrange.Proto.Utility.ProtoHelper"), SF.IdentifierName("ZigZagEncode"));
                arg = SF.InvocationExpression(zigZag).AddArgumentListArguments(SF.Argument(arg));
            }
                
            return SF.ExpressionStatement(SF.InvocationExpression(access).AddArgumentListArguments(SF.Argument(arg)));
        }
            
        private static StatementSyntax[] EmitProtoPackableSerializeStatement(string typeName, string name)
        {
            var measure = SF.MemberAccessExpression(SK.SimpleMemberAccessExpression, SF.IdentifierName(typeName), SF.IdentifierName($"MeasureHandler"));
            var invocation = SF.InvocationExpression(measure).AddArgumentListArguments(SF.Argument(SF.MemberAccessExpression(SK.SimpleMemberAccessExpression, SF.IdentifierName("obj"), SF.IdentifierName(name))));
            var access = SF.MemberAccessExpression(SK.SimpleMemberAccessExpression, SF.IdentifierName("writer"), SF.IdentifierName("EncodeVarInt"));
            var serialize = SF.MemberAccessExpression(SK.SimpleMemberAccessExpression, SF.IdentifierName(typeName), SF.IdentifierName("SerializeHandler"));
            return
            [
                SF.ExpressionStatement(SF.InvocationExpression(access).AddArgumentListArguments(SF.Argument(invocation))),
                SF.ExpressionStatement(SF.InvocationExpression(serialize).AddArgumentListArguments(SF.Argument(SF.MemberAccessExpression(SK.SimpleMemberAccessExpression, SF.IdentifierName("obj"), SF.IdentifierName(name))), SF.Argument(SF.IdentifierName("writer"))))
            ];
        }
            
        private static StatementSyntax EmitBytesSerializeStatement(string name)
        {
            var arg = SF.MemberAccessExpression(SK.SimpleMemberAccessExpression, SF.IdentifierName("obj"), SF.IdentifierName(name));
            var access = SF.MemberAccessExpression(SK.SimpleMemberAccessExpression, SF.IdentifierName("writer"), SF.IdentifierName("EncodeBytes"));
            return SF.ExpressionStatement(SF.InvocationExpression(access).AddArgumentListArguments(SF.Argument(arg)));
        }
            
        private static StatementSyntax EmitStringSerializeStatement(string name)
        {
            var arg = SF.MemberAccessExpression(SK.SimpleMemberAccessExpression, SF.IdentifierName("obj"), SF.IdentifierName(name));
            var access = SF.MemberAccessExpression(SK.SimpleMemberAccessExpression, SF.IdentifierName("writer"), SF.IdentifierName("EncodeString"));
            return SF.ExpressionStatement(SF.InvocationExpression(access).AddArgumentListArguments(SF.Argument(arg)));
        }
            
        private static StatementSyntax EmitNullableCheckStatement(bool equals, string identifier, StatementSyntax statement, bool isBlock = true) => 
            SF.IfStatement(SF.BinaryExpression(equals ? SK.EqualsExpression : SK.NotEqualsExpression, SF.IdentifierName(identifier), SF.LiteralExpression(SK.NullLiteralExpression)), isBlock ? SF.Block(statement) : statement);
            
        private static StatementSyntax EmitDefaultCheckStatement(bool equals, string identifier, StatementSyntax statement, bool isBlock = true) => 
            SF.IfStatement(SF.BinaryExpression(equals ? SK.EqualsExpression : SK.NotEqualsExpression, SF.IdentifierName(identifier), SF.LiteralExpression(SK.DefaultLiteralExpression)), isBlock ? SF.Block(statement) : statement);

        private static AttributeSyntax EmitGeneratedCodeAttribute()
        {
            return SF.Attribute(typeof(GeneratedCodeAttribute).GetFullName())
                .AddArgumentListArguments(
                    SF.AttributeArgument(SF.LiteralExpression(SK.StringLiteralExpression, SF.Literal("Lagrange.Proto.Generator"))),
                    SF.AttributeArgument(SF.LiteralExpression(SK.StringLiteralExpression, SF.Literal("1.0.0"))));
        }
    }
}