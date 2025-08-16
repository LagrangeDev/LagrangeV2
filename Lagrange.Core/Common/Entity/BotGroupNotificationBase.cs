namespace Lagrange.Core.Common.Entity;

public abstract class BotGroupNotificationBase(long groupUin, ulong sequence, BotGroupNotificationType type, long targetUin, string targetUid)
{
    public long GroupUin { get; } = groupUin;

    public ulong Sequence { get; } = sequence;

    public BotGroupNotificationType Type { get; } = type;

    public long TargetUin { get; } = targetUin;

    public string TargetUid { get; } = targetUid;
}
