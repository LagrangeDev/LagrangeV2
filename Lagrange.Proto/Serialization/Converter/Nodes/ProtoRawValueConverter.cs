using Lagrange.Proto.Nodes;
using Lagrange.Proto.Primitives;
using Lagrange.Proto.Utility;

namespace Lagrange.Proto.Serialization.Converter;

internal class ProtoRawValueConverter : ProtoConverter<ProtoRawValue>
{
    public override void Write<TBufferWriter>(int field, WireType wireType, ProtoWriter<TBufferWriter> writer, ProtoRawValue value)
    {
        switch (wireType)
        {
            case WireType.VarInt:
                writer.EncodeVarInt(value.Value);
                break;
            case WireType.Fixed32:
                writer.EncodeFixed32(value.Value);
                break;
            case WireType.Fixed64:
                writer.EncodeFixed64(value.Value);
                break;
            case WireType.LengthDelimited:
                writer.EncodeVarInt(value.Bytes.Length);
                writer.WriteRawBytes(value.Bytes.Span);
                break;
            default:
                throw new NotSupportedException($"Wire type {wireType} is not supported.");
        }
    }

    public override int Measure(int field, WireType wireType, ProtoRawValue value)
    {
        return wireType switch
        {
            WireType.VarInt => ProtoHelper.GetVarIntLength(value.Value),
            WireType.Fixed32 => 4,
            WireType.Fixed64 => 8,
            WireType.LengthDelimited => ProtoHelper.CountBytes(value.Bytes.Span),
            _ => throw new NotSupportedException($"Wire type {wireType} is not supported.")
        };
    }

    public override ProtoRawValue Read(int field, WireType wireType, ref ProtoReader reader)
    {
        if (wireType == WireType.LengthDelimited)
        {
            int length = reader.DecodeVarInt<int>();
            var buffer = GC.AllocateUninitializedArray<byte>(length);
            var span = reader.CreateSpan(length);
            span.CopyTo(buffer);
            
            var memory = new Memory<byte>(buffer);
            return new ProtoRawValue(wireType, 0) { Bytes = memory };
        }

        return wireType switch
        {
            WireType.VarInt => new ProtoRawValue(wireType, reader.DecodeVarInt<long>()),
            WireType.Fixed32 => new ProtoRawValue(wireType, reader.DecodeFixed32<long>()),
            WireType.Fixed64 => new ProtoRawValue(wireType, reader.DecodeFixed64<long>()),
            _ => throw new NotSupportedException($"Wire type {wireType} is not supported.")
        };
    }
}