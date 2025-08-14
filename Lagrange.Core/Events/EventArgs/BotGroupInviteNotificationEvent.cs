using Lagrange.Core.Common.Entity;

namespace Lagrange.Core.Events.EventArgs;

public class BotGroupInviteNotificationEvent(BotGroupInviteNotification notification) : EventBase
{
    public BotGroupInviteNotification Notification { get; } = notification;

    public override string ToEventMessage() => $"{nameof(BotGroupInviteNotificationEvent)} Group {Notification.GroupUin} Inviter {Notification.InviterUin} Target {Notification.TargetUin}";
}