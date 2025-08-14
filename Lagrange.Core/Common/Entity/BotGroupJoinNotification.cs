namespace Lagrange.Core.Common.Entity;

public class BotGroupJoinNotification(long group, ulong sequence, long target, BotGroupNotificationState state, long? @operator, string comment) : BotGroupNotificationBase(group, sequence, BotGroupNotificationType.Join, target)
{
    public BotGroupNotificationState State { get; } = state;

    public long? Operator { get; } = @operator;

    public string Comment { get; } = comment;
}
