using System.Diagnostics.CodeAnalysis;
using Lagrange.Core.Common.Entity;

namespace Lagrange.Core.Internal.Events.System;

internal class FetchGroupNotificationsEventReq(ulong count) : ProtocolEvent
{
    public ulong Count { get; } = count;
}

internal class FetchGroupNotificationsEventResp(List<BotGroupNotificationBase> groupNotifications) : ProtocolEvent
{
    public List<BotGroupNotificationBase> GroupNotifications { get; } = groupNotifications;
}