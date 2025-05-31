using Lagrange.Proto.Nodes;
using Lagrange.Proto.Primitives;
using Lagrange.Proto.Serialization.Metadata;

namespace Lagrange.Proto.Serialization.Converter;

public class ProtoValueConverter : ProtoConverter<ProtoValue>
{
    public override void Write<TBufferWriter>(int field, WireType wireType, ProtoWriter<TBufferWriter> writer, ProtoValue value)
    {
        value.WriteTo(field, writer);
    }

    public override int Measure(int field, WireType wireType, ProtoValue value)
    {
        return value.Measure(field);
    }

    public override ProtoValue Read(int field, WireType wireType, ref ProtoReader reader)
    {
        var value = ProtoTypeResolver.GetConverter<ProtoRawValue>().Read(field, wireType, ref reader);
        return new ProtoValue<ProtoRawValue>(value, wireType);
    }
}