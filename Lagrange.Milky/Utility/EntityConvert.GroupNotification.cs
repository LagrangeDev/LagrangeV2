using Lagrange.Core.Common.Entity;
using Lagrange.Milky.Entity;

namespace Lagrange.Milky.Utility;

public partial class EntityConvert
{
    public GroupNotificationBase? GroupNotification(BotGroupNotificationBase notification)
    {
        return notification switch
        {
            BotGroupJoinNotification no => new GroupJoinRequestNotification(
                    no.GroupUin,
                    (long)no.Sequence,
                    no.IsFiltered,
                    no.TargetUin,
                    no.State switch
                    {
                        BotGroupNotificationState.Wait => "pending",
                        BotGroupNotificationState.Accept => "accepted",
                        BotGroupNotificationState.Reject => "rejected",
                        BotGroupNotificationState.Ignore => "ignored",
                        _ => throw new NotSupportedException(),
                    },
                    no.OperatorUin,
                    no.Comment
                ),
            BotGroupSetAdminNotification no => new GroupAdminChangeNotification(
                no.GroupUin,
                (long)no.Sequence,
                no.TargetUin,
                true,
                no.OperatorUin
            ),
            BotGroupUnsetAdminNotification no => new GroupAdminChangeNotification(
                no.GroupUin,
                (long)no.Sequence,
                no.TargetUin,
                false,
                no.OperatorUin
            ),
            BotGroupKickNotification no => new GroupKickNotification(
                no.GroupUin,
                (long)no.Sequence,
                no.TargetUin,
                no.OperatorUin
            ),
            BotGroupExitNotification no => new GroupQuitNotification(
                no.GroupUin,
                (long)no.Sequence,
                no.TargetUin
            ),
            BotGroupInviteNotification no => new GroupInvitedJoinRequestNotification(
                no.GroupUin,
                (long)no.Sequence,
                no.InviterUin,
                no.TargetUin,
                no.State switch
                {
                    BotGroupNotificationState.Wait => "pending",
                    BotGroupNotificationState.Accept => "accepted",
                    BotGroupNotificationState.Reject => "rejected",
                    BotGroupNotificationState.Ignore => "ignored",
                    _ => throw new NotSupportedException(),
                },
                no.OperatorUin
            ),
            _ => null,
        };
    }
}