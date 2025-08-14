using Lagrange.Proto;

namespace Lagrange.Core.Internal.Packets.Notify;

#pragma warning disable CS8618

[ProtoPackable]
internal partial class GroupJoin
{
    [ProtoMember(1)] public long GroupUin { get; set; }

    [ProtoMember(2)] public ulong Type { get; set; }

    [ProtoMember(3)] public string TargetUid { get; set; }

    [ProtoMember(5)] public string Comment { get; set; }
}