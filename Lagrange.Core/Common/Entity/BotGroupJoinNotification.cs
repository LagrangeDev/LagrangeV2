namespace Lagrange.Core.Common.Entity;

public class BotGroupJoinNotification(long group, ulong sequence, long targetUin, string targetUid, BotGroupNotificationState state, long? operatorUin, string? operatorUid, string comment) : BotGroupNotificationBase(group, sequence, BotGroupNotificationType.Join, targetUin, targetUid)
{
    public BotGroupNotificationState State { get; } = state;

    public long? OperatorUin { get; } = operatorUin;

    public string? OperatorUid { get; } = operatorUid;

    public string Comment { get; } = comment;
}
