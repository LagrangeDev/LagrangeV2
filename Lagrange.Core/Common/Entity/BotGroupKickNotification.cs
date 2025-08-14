namespace Lagrange.Core.Common.Entity;

public class BotGroupKickNotification(long group, ulong sequence, long target, long @operator) : BotGroupNotificationBase(group, sequence, BotGroupNotificationType.Exit, target)
{
    public long Operator { get; } = @operator;
}
