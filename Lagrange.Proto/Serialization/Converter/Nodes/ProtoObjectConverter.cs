using Lagrange.Proto.Nodes;
using Lagrange.Proto.Primitives;

namespace Lagrange.Proto.Serialization.Converter;

public class ProtoObjectConverter : ProtoConverter<ProtoObject>
{
    public override void Write<TBufferWriter>(int field, WireType wireType, ProtoWriter<TBufferWriter> writer, ProtoObject value)
    {
        value.WriteTo(field, writer);
    }

    public override int Measure(int field, WireType wireType, ProtoObject value)
    {
        return value.Measure(field);
    }

    public override ProtoObject Read(int field, WireType wireType, ref ProtoReader reader)
    {
        int length = reader.DecodeVarInt<int>();
        var subSpan = reader.CreateSpan(length);
        return ProtoObject.Parse(subSpan);
    }
}