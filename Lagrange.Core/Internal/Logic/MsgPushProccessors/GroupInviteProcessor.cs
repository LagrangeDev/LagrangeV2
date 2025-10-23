using Lagrange.Core.Common.Entity;
using Lagrange.Core.Events.EventArgs;
using Lagrange.Core.Internal.Events.Message;
using Lagrange.Core.Internal.Events.System;
using Lagrange.Core.Internal.Packets.Notify;
using Lagrange.Core.Utility;

namespace Lagrange.Core.Internal.Logic.MsgPushProccessors;

[MsgPushProcessor(MsgType.Event0x20D, true)]
internal class GroupInviteProcessor : MsgPushProcessorBase
{
    // another in `RichTextMsgProcessor` for private send invitation card.
    internal override async ValueTask<bool> Handle(BotContext context, MsgType msgType, int subType,
        PushMessageEvent msgEvt, ReadOnlyMemory<byte>? content)
    {
        var @event = ProtoHelper.Deserialize<Event0x20D>(content!.Value.Span);
        if (@event.SubType != 87) return false; // GroupInviteNotification

        var body = ProtoHelper.Deserialize<GroupInvite>(@event.Body);

        var response = await context.EventContext.SendEvent<FetchGroupNotificationsEventResp>(
            new FetchGroupNotificationsEventReq(20)
        );
        var inviteNotifications = response
            .GroupNotifications
            .OfType<BotGroupInviteNotification>();
        var notification = inviteNotifications.FirstOrDefault(notification =>
            body.Body.GroupUin == notification.GroupUin &&
            body.Body.InviterUid == notification.InviterUid &&
            body.Body.TargetUid == notification.TargetUid &&
            notification.State == BotGroupNotificationState.Wait
        );
        if (notification == null)
        {
            context.LogWarning(nameof(PushLogic),
                "Received GroupInviteNotification but no corresponding notification found");
            return false;
        }

        context.EventInvoker.PostEvent(new BotGroupInviteNotificationEvent(notification));
        return true;
    }
}