using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Lagrange.Proto.Serialization;

namespace Lagrange.Proto;

[StackTraceHidden]
[DebuggerStepThrough]
[ExcludeFromCodeCoverage] // This class is used to throw exceptions in a way that allows the compiler to optimize the code better.
internal static class ThrowHelper
{
    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void ThrowInvalidOperationException_NeedLargerSpan() => throw new InvalidOperationException("Need larger span");
    
    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void ThrowInvalidOperationException_InvalidNumberHandling(Type type) => throw new InvalidOperationException($"Invalid number handling for the type {type.Name}");
    
    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void ThrowInvalidOperationException_FailedDetermineConverter<T>() => throw new InvalidOperationException($"Unable to determine the type of the object to serialize for {typeof(T).Name}");
    
    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void ThrowInvalidDataException_MalformedMessage() => throw new InvalidDataException("Malformed proto message while decoding");

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void ThrowObjectDisposedException_ProtoWriter() => throw new ObjectDisposedException("ProtoWriter", "The ProtoWriter has been disposed. Please ensure that the ProtoWriter is not used after it has been disposed.");
    
    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void ThrowArgumentOutOfRangeException_NoEnoughSpace(string type, int size, int available) => throw new ArgumentOutOfRangeException(type, $"The {type} size is {size}, but only {available} bytes are available.");

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void ThrowInvalidOperationException_DuplicateField(Type type, int fieldInfoField) => throw new InvalidOperationException($"The type {type.Name} has duplicate field {fieldInfoField}. Please ensure that the field numbers are unique.");

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void ThrowInvalidOperationException_CanNotCreateObject(Type type) => throw new InvalidOperationException($"Cannot create an instance of {type.Name}. Ensure that the type has a parameterless constructor or is a value type.");
    
    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void ThrowInvalidOperationException_NodeWrongType(params ReadOnlySpan<string> supportedTypeNames)
    {
        Debug.Assert(supportedTypeNames.Length > 0);
        string concatenatedNames = supportedTypeNames.Length == 1 ? supportedTypeNames[0] : string.Join(", ", supportedTypeNames.ToArray());
        throw new InvalidOperationException($"The node is not of the expected type. Supported types are: {concatenatedNames}.");
    }

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void ThrowInvalidOperationException_NodeCycleDetected() => throw new InvalidOperationException("Node cycle detected");

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void ThrowInvalidOperationException_NodeAlreadyHasParent() => throw new InvalidOperationException("Node already has a parent");
    
    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void ThrowInvalidOperationException_InvalidWireType(WireType wireType) => throw new InvalidOperationException($"Invalid wire type {wireType} for the node.");
    
    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void ThrowInvalidOperationException_InvalidNodesWireType(string fieldName) => throw new InvalidOperationException($"The wire type must be explicitly set for field {fieldName} as the wire type for the ProtoNode, ProtoValue, and ProtoArray types is not known at compile time, to set the wire type, use the NodesWireType Property in ProtoMember attribute");

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void ThrowInvalidOperationException_NullPolymorphicDiscriminator(Type type) => throw new InvalidOperationException($"The polymorphic discriminator field for type {type.Name} cannot be null. Please ensure that the field is set and has a valid value.");
    
    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void ThrowInvalidOperationException_DuplicatePolymorphicDiscriminator(Type type, object key) => throw new InvalidOperationException($"The polymorphic discriminator key '{key}' for type {type.Name} is duplicated. Please ensure that the keys are unique.");
    
    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void ThrowInvalidOperationException_PolymorphicFieldNotFirst(Type type, uint expected, uint actual) => throw new InvalidOperationException($"The polymorphic discriminator field for type {type.Name} must be the first field in the message. Expected field number {expected}, but found {actual}.");
    
    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void ThrowInvalidOperationException_FailedParsePolymorphicType(Type type, uint index) => throw new InvalidOperationException($"Failed to parse the polymorphic type from proto for type {type.Name} at '{index}'.");

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void ThrowInvalidOperationException_UnknownPolymorphicType(Type type, object polyTypeKey) => throw new InvalidOperationException($"Unknown polymorphic type '{polyTypeKey}' for base type {type.Name}. Please ensure that the polymorphic type is registered.");
}