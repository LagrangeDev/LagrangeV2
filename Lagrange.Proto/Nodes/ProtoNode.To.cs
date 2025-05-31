using System.Buffers;
using Lagrange.Proto.Primitives;

namespace Lagrange.Proto.Nodes;

public partial class ProtoNode
{
    public abstract void WriteTo<TBufferWriter>(int field, ProtoWriter<TBufferWriter> writer) where TBufferWriter : struct, IBufferWriter<byte>;

    public abstract int Measure(int field);
}