using Lagrange.Proto;

namespace Lagrange.Core.Internal.Packets.Notify;

#pragma warning disable CS8618

[ProtoPackable]
internal partial class Event0x20D
{
    [ProtoMember(1)] public ulong SubType { get; set; }

    [ProtoMember(2)] public byte[] Body { get; set; }
}