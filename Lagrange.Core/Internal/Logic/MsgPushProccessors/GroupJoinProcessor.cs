using Lagrange.Core.Common.Entity;
using Lagrange.Core.Events.EventArgs;
using Lagrange.Core.Internal.Events.Message;
using Lagrange.Core.Internal.Events.System;
using Lagrange.Core.Internal.Packets.Notify;
using Lagrange.Core.Utility;

namespace Lagrange.Core.Internal.Logic.MsgPushProccessors;

[MsgPushProcessor(MsgType.GroupJoinNotification, true)]
internal class GroupJoinProcessor : MsgPushProcessorBase
{
    internal override async ValueTask<bool> Handle(BotContext context, MsgType msgType, int subType, PushMessageEvent msgEvt, ReadOnlyMemory<byte>? content)
    {
        var join = ProtoHelper.Deserialize<GroupJoin>(content!.Value.Span);
        var response = await context.EventContext.SendEvent<FetchGroupNotificationsEventResp>(
            new FetchGroupNotificationsEventReq(20)
        );
        var joinNotifications = response
            .GroupNotifications
            .OfType<BotGroupJoinNotification>();
        var notification = joinNotifications.FirstOrDefault(notification =>
            join.GroupUin == notification.GroupUin &&
            join.TargetUid == notification.TargetUid &&
            notification.State == BotGroupNotificationState.Wait
        );
        if (notification == null)
        {
            context.LogWarning(nameof(PushLogic), "Received GroupJoinNotification but no corresponding notification found");
            return false;
        }

        context.EventInvoker.PostEvent(new BotGroupJoinNotificationEvent(notification));
        return true;
    }
}