namespace Lagrange.Core.Common.Entity;

public class BotGroupInviteNotification(long group, ulong sequence, long target, BotGroupNotificationState state, long? @operator, long inviter) : BotGroupNotificationBase(group, sequence, BotGroupNotificationType.Invite, target)
{
    public BotGroupNotificationState State { get; } = state;

    public long? Operator { get; } = @operator;

    public long Inviter { get; } = inviter;
}
