using System.Text;
using Lagrange.Core.Common.Entity;
using Lagrange.Core.Events.EventArgs;
using Lagrange.Core.Internal.Events.Message;
using Lagrange.Core.Internal.Events.System;
using Lagrange.Core.Internal.Packets.Notify;
using Lagrange.Core.Utility;

namespace Lagrange.Core.Internal.Logic.MsgPushProccessors;

[MsgPushProcessor(MsgType.GroupMemberIncreaseNotice, true)]
internal class GroupMemberIncreaseProcessor : MsgPushProcessorBase
{
    internal override async ValueTask<bool> Handle(BotContext context, MsgType msgType, int subType, PushMessageEvent msgEvt, ReadOnlyMemory<byte>? content)
    {
        var increase = ProtoHelper.Deserialize<GroupChange>(content!.Value.Span);
        var response = await context.EventContext.SendEvent<FetchGroupNotificationsEventResp>(
            new FetchGroupNotificationsEventReq(20)
        );
        var inviteNotifications = response
            .GroupNotifications
            .OfType<BotGroupInviteNotification>();
        var notification = inviteNotifications.FirstOrDefault(notification =>
            increase.GroupUin == notification.GroupUin &&
            increase.MemberUid == notification.TargetUid &&
            notification.State == BotGroupNotificationState.Accept
        );
        var operatorUin = notification?.OperatorUin;
        context.EventInvoker.PostEvent(new BotGroupMemberIncreaseEvent(
            increase.GroupUin,
            (await context.CacheContext.ResolveGroupMember(increase.GroupUin, increase.MemberUid))?.Uin ?? 0,
            (await context.CacheContext.ResolveGroupMember(increase.GroupUin, Encoding.UTF8.GetString(increase.Operator.AsSpan())))?.Uin ?? 0,
            increase.IncreaseType,
            operatorUin));
        return true;
    }
}