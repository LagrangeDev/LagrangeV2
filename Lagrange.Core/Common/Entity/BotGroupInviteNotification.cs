namespace Lagrange.Core.Common.Entity;

public class BotGroupInviteNotification(long group, ulong sequence, long targetUin, string targetUid, BotGroupNotificationState state, long? operatorUin, string? operatorUid, long inviterUin, string inviterUid) : BotGroupNotificationBase(group, sequence, BotGroupNotificationType.Invite, targetUin, targetUid)
{
    public BotGroupNotificationState State { get; } = state;

    public long? OperatorUin { get; } = operatorUin;

    public string? OperatorUid { get; } = operatorUid;

    public long InviterUin { get; } = inviterUin;

    public string InviterUid { get; } = inviterUid;
}
