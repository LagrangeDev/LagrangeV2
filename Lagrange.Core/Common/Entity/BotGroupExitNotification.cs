namespace Lagrange.Core.Common.Entity;

public class BotGroupExitNotification(long group, ulong sequence, long target) : BotGroupNotificationBase(group, sequence, BotGroupNotificationType.Exit, target);