namespace Lagrange.Core.Common.Entity;

public abstract class BotGroupNotificationBase(long group, ulong sequence, BotGroupNotificationType type, long target)
{
    public long Group { get; } = group;

    public ulong Sequence { get; } = sequence;

    public BotGroupNotificationType Type { get; } = type;

    public long Target { get; } = target;
}
