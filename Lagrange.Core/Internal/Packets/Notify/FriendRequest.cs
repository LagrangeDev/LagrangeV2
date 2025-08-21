using Lagrange.Proto;

namespace Lagrange.Core.Internal.Packets.Notify;

#pragma warning disable CS8618

[ProtoPackable]
internal partial class FriendRequest
{
    [ProtoMember(1)] public FriendRequestInfo? Info { get; set; }
}

[ProtoPackable]
internal partial class FriendRequestInfo
{
    [ProtoMember(1)] public string TargetUid { get; set; }

    [ProtoMember(2)] public string SourceUid { get; set; }

    [ProtoMember(5)] public string NewSource { get; set; }

    [ProtoMember(10)] public string Message { get; set; }

    [ProtoMember(11)] public string? Source { get; set; }
}
