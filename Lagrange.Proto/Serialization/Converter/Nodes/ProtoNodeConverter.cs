using Lagrange.Proto.Nodes;
using Lagrange.Proto.Primitives;
using Lagrange.Proto.Serialization.Metadata;

namespace Lagrange.Proto.Serialization.Converter;

public class ProtoNodeConverter : ProtoConverter<ProtoNode>
{
    public override void Write<TBufferWriter>(int field, WireType wireType, ProtoWriter<TBufferWriter> writer, ProtoNode value)
    {
        value.WriteTo(field, writer);
    }

    public override int Measure(int field, WireType wireType, ProtoNode value)
    {
        return value.Measure(field);
    }

    public override ProtoNode Read(int field, WireType wireType, ref ProtoReader reader)
    {
        var value = ProtoTypeResolver.GetConverter<ProtoRawValue>().Read(field, wireType, ref reader);
        return new ProtoValue<ProtoRawValue>(value, wireType);
    }
}