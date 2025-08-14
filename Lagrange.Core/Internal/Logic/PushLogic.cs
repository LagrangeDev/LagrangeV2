using System.Text;
using Lagrange.Core.Common;
using Lagrange.Core.Common.Entity;
using Lagrange.Core.Events.EventArgs;
using Lagrange.Core.Internal.Events;
using Lagrange.Core.Internal.Events.Message;
using Lagrange.Core.Internal.Events.System;
using Lagrange.Core.Internal.Packets.Notify;
using Lagrange.Core.Utility.Binary;
using ProtoHelper = Lagrange.Core.Utility.ProtoHelper;

namespace Lagrange.Core.Internal.Logic;

[EventSubscribe<PushMessageEvent>(Protocols.All)]
internal class PushLogic(BotContext context) : ILogic
{
    public async ValueTask Incoming(ProtocolEvent e)
    {
        var messageEvent = (PushMessageEvent)e;

        switch ((Type)messageEvent.MsgPush.CommonMessage.ContentHead.Type)
        {
            case Type.GroupMessage:
            case Type.PrivateMessage:
            case Type.TempMessage:
            {
                var message = await context.EventContext.GetLogic<MessagingLogic>().Parse(messageEvent.MsgPush.CommonMessage);
                context.EventInvoker.PostEvent(new BotMessageEvent(message, messageEvent.Raw));
                break;
            }
            case Type.GroupMemberDecreaseNotice when messageEvent.MsgPush.CommonMessage.MessageBody.MsgContent is { } content:
            {
                var decrease = ProtoHelper.Deserialize<GroupChange>(content.Span);
                if (decrease.DecreaseType == 3)
                {
                    var op = ProtoHelper.Deserialize<OperatorInfo>(decrease.Operator.AsSpan());
                    context.EventInvoker.PostEvent(
                        new BotGroupMemberDecreaseEvent(
                            decrease.GroupUin,
                            context.CacheContext.ResolveUin(decrease.MemberUid),
                            context.CacheContext.ResolveUin(op.Operator.Uid ?? "")
                        )
                    );
                }
                else
                {
                    context.EventInvoker.PostEvent(
                        new BotGroupMemberDecreaseEvent(
                            decrease.GroupUin,
                            context.CacheContext.ResolveUin(decrease.MemberUid),
                            context.CacheContext.ResolveUin(Encoding.UTF8.GetString(decrease.Operator.AsSpan()))
                        )
                    );
                }
                break;
            }
            case Type.GroupJoinNotification when messageEvent.MsgPush.CommonMessage.MessageBody.MsgContent is { } content:
            {
                var join = ProtoHelper.Deserialize<GroupJoin>(content.Span);

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
                    break;
                }

                context.EventInvoker.PostEvent(new BotGroupJoinNotificationEvent(notification));
                break;
            }
            case Type.Event0x20D when messageEvent.MsgPush.CommonMessage.MessageBody.MsgContent is { } content:
            {
                var decrease = ProtoHelper.Deserialize<Event0x20D>(content.Span);
                switch ((Event0x20DSubType)decrease.SubType)
                {
                    case Event0x20DSubType.GroupInviteNotification:
                    {
                        var body = ProtoHelper.Deserialize<GroupInvite>(decrease.Body);

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
                            context.LogWarning(nameof(PushLogic), "Received GroupInviteNotification but no corresponding notification found");
                            break;
                        }

                        context.EventInvoker.PostEvent(new BotGroupInviteNotificationEvent(notification));
                        break;
                    }
                }
                break;
            }
            case Type.Event0x2DC:
            {
                var pkgType = (Event0x2DCSubType)messageEvent.MsgPush.CommonMessage.ContentHead.SubType;
                switch (pkgType)
                {
                    case Event0x2DCSubType.GroupGreyTipNotice20 when messageEvent.MsgPush.CommonMessage.MessageBody.MsgContent is { } content:
                        var packet = new BinaryPacket(content);
                        Int64 groupUin = packet.Read<Int32>(); // group uin
                        _ = packet.Read<byte>(); // unknown byte
                        var proto = packet.ReadBytes(Prefix.Int16 | Prefix.LengthOnly);
                        var greyTip = ProtoHelper.Deserialize<NotifyMessageBody>(proto);
                        var templates = greyTip.GeneralGrayTip.MsgTemplParam.ToDictionary(x => x.Name, x => x.Value);

                        if (!templates.TryGetValue("action_str", out var actionStr) && !templates.TryGetValue("alt_str1", out actionStr))
                        {
                            actionStr = string.Empty;
                        }

                        if (greyTip.GeneralGrayTip.BusiType == 12) // poke
                        {
                            context.EventInvoker.PostEvent(new BotGroupNudgeEvent(
                                groupUin,
                                uint.Parse(templates["uin_str1"]),
                                uint.Parse(templates["uin_str2"]))
                            );
                        }
                        break;
                }
                break;
            }
            default: break;
        }
    }

    private enum Type
    {
        GroupMemberDecreaseNotice = 34,
        GroupMessage = 82,
        GroupJoinNotification = 84,
        TempMessage = 141,
        PrivateMessage = 166,
        Event0x20D = 525,
        Event0x210 = 528,  // friend related event
        Event0x2DC = 732,  // group related event
    }

    private enum Event0x20DSubType
    {
        GroupInviteNotification = 87
    }

    private enum Event0x2DCSubType
    {
        GroupMuteNotice = 12,
        SubType16 = 16,
        GroupRecallNotice = 17,
        GroupGreyTipNotice21 = 21,
        GroupGreyTipNotice20 = 20,
    }

    private enum Event0x2DCSubType16Field13
    {
        GroupMemberSpecialTitleNotice = 6,
        GroupNameChangeNotice = 12,
        GroupTodoNotice = 23,
        GroupReactionNotice = 35,
    }
}
