using Lagrange.Proto;

namespace Lagrange.Core.Internal.Packets.Service;

#pragma warning disable CS8618

[ProtoPackable]
internal partial class FetchGroupNotificationsRequest
{
    [ProtoMember(1)] public ulong Count { get; set; }

    [ProtoMember(2)] public ulong StartSequence { get; set; }
}


[ProtoPackable]
internal partial class FetchGroupNotificationsResponse
{
    // If empty, it is null
    [ProtoMember(1)] public List<FetchGroupNotificationsResponseNotification>? GroupNotifications { get; set; }
}

[ProtoPackable]
public partial class FetchGroupNotificationsResponseNotification
{
    [ProtoMember(1)] public ulong Sequence { get; set; }

    // 1 join 6 kick other(no state) 7 kick self(no state) 13 exit(no state) 22 invite
    [ProtoMember(2)] public ulong Type { get; set; }

    // 1 wait 2 accept 3 reject 4 ignore
    [ProtoMember(3)] public ulong State { get; set; }

    [ProtoMember(4)] public FetchGroupNotificationsResponseNotificationGroup Group { get; set; }

    [ProtoMember(5)] public FetchGroupNotificationsResponseNotificationUser Target { get; set; }

    [ProtoMember(6)] public FetchGroupNotificationsResponseNotificationUser Inviter { get; set; }

    [ProtoMember(7)] public FetchGroupNotificationsResponseNotificationUser Operator { get; set; }

    [ProtoMember(10)] public string Comment { get; set; }
}

[ProtoPackable]
public partial class FetchGroupNotificationsResponseNotificationGroup
{
    [ProtoMember(1)] public long GroupUin { get; set; }
}

[ProtoPackable]
public partial class FetchGroupNotificationsResponseNotificationUser
{
    [ProtoMember(1)] public string Uid { get; set; }
}