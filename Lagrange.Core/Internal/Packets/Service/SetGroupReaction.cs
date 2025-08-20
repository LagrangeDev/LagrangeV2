using Lagrange.Proto;

namespace Lagrange.Core.Internal.Packets.Service;

#pragma warning disable CS8618

[ProtoPackable]
public partial class SetGroupReactionRequest
{
    [ProtoMember(2)] public long GroupUin { get; set; }

    [ProtoMember(3)] public ulong Sequence { get; set; }

    [ProtoMember(4)] public string Code { get; set; }

    [ProtoMember(5)] public ulong Type { get; set; }
}

[ProtoPackable]
public partial class SetGroupReactionResponse;
