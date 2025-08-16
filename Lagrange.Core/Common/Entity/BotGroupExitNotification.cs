namespace Lagrange.Core.Common.Entity;

public class BotGroupExitNotification(long groupUin, ulong sequence, long targetUin, string targetUid) : BotGroupNotificationBase(groupUin, sequence, BotGroupNotificationType.Exit, targetUin, targetUid);