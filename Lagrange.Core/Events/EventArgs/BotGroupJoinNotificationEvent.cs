using Lagrange.Core.Common.Entity;

namespace Lagrange.Core.Events.EventArgs;

public class BotGroupJoinNotificationEvent(BotGroupJoinNotification notification) : EventBase
{
    public BotGroupJoinNotification Notification { get; } = notification;

    public override string ToEventMessage() => $"{nameof(BotGroupJoinNotificationEvent)} Group {Notification.GroupUin} Target {Notification.TargetUin}";
}