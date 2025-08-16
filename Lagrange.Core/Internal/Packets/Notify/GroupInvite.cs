using Lagrange.Proto;

namespace Lagrange.Core.Internal.Packets.Notify;

#pragma warning disable CS8618

[ProtoPackable]
internal partial class GroupInvite
{
    [ProtoMember(1)] public GroupInviteBody Body { get; set; }
}

[ProtoPackable]
internal partial class GroupInviteBody
{
    [ProtoMember(1)] public long GroupUin { get; set; }

    [ProtoMember(5)] public string TargetUid { get; set; }

    [ProtoMember(6)] public string InviterUid { get; set; }
}