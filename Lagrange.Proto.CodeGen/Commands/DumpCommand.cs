using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Loader;
using Lagrange.Proto.CodeGen.Format;
using Lagrange.Proto.Serialization;

namespace Lagrange.Proto.CodeGen.Commands;

public static class DumpCommand
{
    [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "Dynamic assembly loading")]
    public static async Task Invoke(string path)
    {
        var protoFiles = new Dictionary<string, ProtoFile>();
        
        try
        {
            string file = Path.GetFileName(path);
            var context = new AssemblyLoadContext(file, true);
            var assembly = context.LoadFromAssemblyPath(path);

            foreach (var type in assembly.GetTypes())
            {
                var attributes = type.GetCustomAttribute<ProtoPackableAttribute>();
                if (attributes == null) continue;

                var namespaces = type.Namespace?.Split('.') ?? [];
                string ns = string.Join('.', namespaces);
                if (!protoFiles.TryGetValue(ns, out var protoFile))
                {
                    protoFile = new ProtoFile
                    {
                        Syntax = "proto2",
                        Package = ns
                    };
                    protoFiles[ns] = protoFile;
                }

                var message = GenerateMessage(type);
                protoFile.Messages.Add(message);
            }
            
            foreach (var proto in protoFiles.Values)
            {
                string fileName = Path.Combine(Path.GetDirectoryName(path) ?? "", $"{proto.Package}.proto");
                await using var writer = new StreamWriter(fileName);
                await writer.WriteLineAsync(new ProtoWriter().WriteProto(proto));
                
                Console.WriteLine($"Proto file generated: {fileName}");
            }
            
            Console.WriteLine($"Proto files generated in {Path.GetDirectoryName(path)}");
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error: {e.Message}");
            if (e.StackTrace is not null) Console.WriteLine(e.StackTrace);
        }
    }

    [UnconditionalSuppressMessage("Trimming", "IL2070", Justification = "Dynamic assembly loading")]
    private static ProtoMessage GenerateMessage(Type type)
    {
        var message = new ProtoMessage { Name = type.Name };
        foreach (var member in type.GetMembers())
        {
            var memberAttribute = member.GetCustomAttribute<ProtoMemberAttribute>();
            if (memberAttribute != null)
            {
                var fieldType = (member as PropertyInfo)?.PropertyType ?? (member as FieldInfo)?.FieldType ?? throw new InvalidOperationException();
                bool fixedSize = (memberAttribute.NumberHandling & ~ProtoNumberHandling.Fixed32) != ProtoNumberHandling.Default && (memberAttribute.NumberHandling & ~ProtoNumberHandling.Fixed64) != ProtoNumberHandling.Default;
                bool signed = (memberAttribute.NumberHandling & ~ProtoNumberHandling.Signed) != ProtoNumberHandling.Default;
                string protoType = ConvertToProtoType(member, fieldType, fixedSize, signed, out string modifier);
                var field = new ProtoField
                {
                    Name = member.Name,
                    Type = string.IsNullOrEmpty(modifier) ? protoType : $"{modifier} {protoType}",
                    Number = memberAttribute.Field,
                };
                message.Fields.Add(field);
            }
        }

        return message;
    }

    private static readonly Type[] Generics =
    [
        typeof(Nullable<>),
        typeof(List<>)
    ];
    
    private static readonly Type[] StringTypes =
    [
        typeof(string),
        typeof(ReadOnlySpan<char>),
        typeof(Span<char>),
        typeof(ReadOnlyMemory<char>),
        typeof(Memory<char>)
    ];
    
    private static readonly Type[] BytesTypes =
    [
        typeof(byte[]),
        typeof(ReadOnlySpan<byte>),
        typeof(Span<byte>),
        typeof(ReadOnlyMemory<byte>),
        typeof(Memory<byte>)
    ];
    
    private static string ConvertToProtoType(MemberInfo member, Type type, bool fixedSize, bool signed, out string modifier)
    {
        modifier = "optional";
        
        if (StringTypes.Contains(type)) return "string";
        if (BytesTypes.Contains(type)) return "bytes";
        if (type == typeof(bool)) return "bool";
        if (type == typeof(float)) return "float";
        if (type == typeof(double)) return "double";
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>)) return ConvertToProtoType(member, type.GenericTypeArguments[0], fixedSize, signed, out modifier);
        
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Dictionary<,>))
        {
            modifier = string.Empty;
            
            var args = type.GenericTypeArguments;
            if (args.Length != 2) throw new InvalidOperationException($"Invalid dictionary type: {type}");
            var attribute = member.GetCustomAttribute<ProtoValueMemberAttribute>();
            bool valueFixedSize = false;
            bool valueSigned = false;
            if (attribute != null)
            {
                valueFixedSize = (attribute.NumberHandling & ~ProtoNumberHandling.Fixed32) != ProtoNumberHandling.Default && (attribute.NumberHandling & ~ProtoNumberHandling.Fixed64) != ProtoNumberHandling.Default;
                valueSigned = (attribute.NumberHandling & ~ProtoNumberHandling.Signed) != ProtoNumberHandling.Default;
            }
            
            string keyType = ConvertToProtoType(member, args[0], fixedSize, signed, out _);
            string valueType = ConvertToProtoType(member, args[1], valueFixedSize, valueSigned, out _);
            return $"map<{keyType}, {valueType}>";
        }
        
        if (type.IsArray)
        {
            modifier = "repeated";
            return ConvertToProtoType(member, type.GetElementType() ?? throw new InvalidOperationException(), fixedSize, signed, out _);
        }
        if (type.IsGenericType && Generics.Contains(type.GetGenericTypeDefinition()))
        {
            modifier = "repeated";
            return ConvertToProtoType(member, type.GenericTypeArguments[0], fixedSize, signed, out _);
        }

        if (type.IsValueType)
        {
            if (type.IsEnum)
            {
                type = Enum.GetUnderlyingType(type);
            }
            
            int size = Marshal.SizeOf(type);
            bool isLong = size > 4;
            
            if (signed)
            {
                return fixedSize 
                    ? isLong ? "sfixed64" : "sfixed32" 
                    : isLong ? "sint64" : "sint32";
            }
            else
            {
                if (fixedSize)
                {
                    return isLong ? "fixed64" : "fixed32";
                }
                else
                {
                    bool isUnsigned = (byte)Type.GetTypeCode(type) % 2 == 0 && Type.GetTypeCode(type) is >= TypeCode.Byte and <= TypeCode.UInt64;
                    return isUnsigned 
                        ? isLong ? "uint64" : "uint32" 
                        : isLong ? "int64" : "int32";
                }
            }
        }


        return type.Name;
    }
}