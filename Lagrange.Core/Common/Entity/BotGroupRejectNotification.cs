namespace Lagrange.Core.Common.Entity;

public class BotGroupUnsetAdminNotification(long groupUin, ulong sequence, long targetUin, string targetUid, long operatorUin, string operatorUid) : BotGroupNotificationBase(groupUin, sequence, BotGroupNotificationType.UnsetAdmin, targetUin, targetUid)
{
    public long Operator { get; } = operatorUin;

    public string OperatorUid { get; } = operatorUid;
}
