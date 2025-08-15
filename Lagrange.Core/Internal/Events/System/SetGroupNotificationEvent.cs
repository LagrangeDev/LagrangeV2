using Lagrange.Core.Common.Entity;

namespace Lagrange.Core.Internal.Events.System;

internal class SetGroupNotificationEventReq(long groupUin, ulong sequence, BotGroupNotificationType type, GroupNotificationOperate operate, string message) : ProtocolEvent
{
    public long GroupUin { get; } = groupUin;

    public ulong Sequence { get; } = sequence;

    public BotGroupNotificationType Type { get; } = type;

    public GroupNotificationOperate Operate { get; } = operate;

    public string Message { get; } = message;
}

internal class SetGroupNotificationEventResp : ProtocolEvent
{
    public static readonly SetGroupNotificationEventResp Default = new();
}