using Lagrange.Proto;

namespace Lagrange.Core.Internal.Packets.Notify;

#pragma warning disable CS8618

[ProtoPackable]
internal partial class GroupReactionData0
{
    [ProtoMember(1)] public GroupReactionData1 Data { get; set; }
}

[ProtoPackable]
internal partial class GroupReactionData1
{
    [ProtoMember(1)] public GroupReactionData2 Data { get; set; }
}

[ProtoPackable]
internal partial class GroupReactionData2
{
    [ProtoMember(2)] public GroupReactionTarget Target { get; set; }

    [ProtoMember(3)] public GroupReactionData3 Data { get; set; }
}

[ProtoPackable]
internal partial class GroupReactionTarget
{
    [ProtoMember(1)] public ulong Sequence { get; set; }
}

[ProtoPackable]
internal partial class GroupReactionData3
{
    [ProtoMember(1)] public string Code { get; set; }

    [ProtoMember(3)] public uint CurrentCount { get; set; }

    [ProtoMember(4)] public string OperatorUid { get; set; }

    [ProtoMember(5)] public uint Type { get; set; } // 1 Add 2 Remove
}
