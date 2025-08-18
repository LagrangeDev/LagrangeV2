using System.Text.Json;
using System.Web;
using Lagrange.Core.Common;
using Lagrange.Core.Common.Entity;
using Lagrange.Core.Events.EventArgs;
using Lagrange.Core.Internal.Events;
using Lagrange.Core.Internal.Events.Message;
using Lagrange.Core.Internal.Events.System;
using Lagrange.Core.Internal.Packets.Notify;
using Lagrange.Core.Message.Entities;
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
                if (message.Entities[0] is LightAppEntity {AppName: "com.tencent.qun.invite"} || message.Entities[0] is LightAppEntity {AppName: "com.tencent.tuwen.lua"}) 
                {
                    var app = (LightAppEntity)message.Entities[0];
                    using var document = JsonDocument.Parse(app.Payload);
                    var root = document.RootElement;

                    string url = root.GetProperty("meta").GetProperty("news").GetProperty("jumpUrl").GetString() ?? throw new Exception("sb tx! Is this 'com.tencent.qun.invite' or 'com.tencent.tuwen.lua'?");
                    var query = HttpUtility.ParseQueryString(new Uri(url).Query);
                    long groupUin = uint.Parse(query["groupcode"] ?? throw new Exception("sb tx! Is this '/group/invite_join'?"));
                    long sequence = long.Parse(query["msgseq"] ?? throw new Exception("sb tx! Is this '/group/invite_join'?"));
                    context.EventInvoker.PostEvent(new BotGroupInviteSelfEvent(
                        sequence,
                        message.Contact.Uin,
                        groupUin
                    ));
                    break;
                }
                context.EventInvoker.PostEvent(new BotMessageEvent(message, messageEvent.Raw));
                break;
            }
            case Type.GroupMemberDecreaseNotice when messageEvent.MsgPush.CommonMessage.MessageBody.MsgContent is { } content:
            {
                var decrease = ProtoHelper.Deserialize<GroupChange>(content.Span);
                switch ((DecreaseType)decrease.DecreaseType)
                {
                    case DecreaseType.KickSelf:
                    {
                        var op = ProtoHelper.Deserialize<OperatorInfo>(decrease.Operator.AsSpan());
                        context.EventInvoker.PostEvent(new BotGroupMemberDecreaseEvent(
                            decrease.GroupUin,
                            context.CacheContext.ResolveUin(decrease.MemberUid),
                            op.Operator.Uid != null ? context.CacheContext.ResolveUin(op.Operator.Uid) : null
                        ));
                        break;
                    }
                    case DecreaseType.Exit:
                    {
                        await context.CacheContext.GetMemberList(decrease.GroupUin);
                        context.EventInvoker.PostEvent(new BotGroupMemberDecreaseEvent(
                            decrease.GroupUin,
                            context.CacheContext.ResolveUin(decrease.MemberUid),
                            null
                        ));
                        break;
                    }
                    case DecreaseType.Kick:
                    {
                        await context.CacheContext.GetMemberList(decrease.GroupUin);
                        goto case DecreaseType.KickSelf;
                    }
                    default:
                    {
                        context.LogWarning(nameof(PushLogic), "Unknown decrease type: {0}", null, decrease.DecreaseType);
                        break;
                    }
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
                var @event = ProtoHelper.Deserialize<Event0x20D>(content.Span);
                switch ((Event0x20DSubType)@event.SubType)
                {
                    case Event0x20DSubType.GroupInviteNotification:
                    {
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
                            context.LogWarning(nameof(PushLogic), "Received GroupInviteNotification but no corresponding notification found");
                            break;
                        }

                        context.EventInvoker.PostEvent(new BotGroupInviteNotificationEvent(notification));
                        break;
                    }
                    default:
                    {
                        context.LogWarning(nameof(PushLogic), "Unknown 0x20D sub type: {0}", null, @event.SubType);
                        break;
                    }
                }
                break;
            }
            case Type.Event0x210:
            {
                var pkgType210 = (Event0x210SubType)messageEvent.MsgPush.CommonMessage.ContentHead.SubType;
                switch (pkgType210)
                {
                    case Event0x210SubType.FriendRequestNotice
                        when messageEvent.MsgPush.CommonMessage.MessageBody.MsgContent is { } content:
                        var friendRequest = ProtoHelper.Deserialize<FriendRequest>(content.Span);
                        context.EventInvoker.PostEvent(new BotFriendRequestEvent(
                            friendRequest.Info!.SourceUid,
                            messageEvent.MsgPush.CommonMessage.RoutingHead.FromUin,
                            friendRequest.Info.Message,
                            friendRequest.Info.Source ?? string.Empty
                        ));
                        break;
                }

                break;
            }
            case Type.Event0x2DC:
            {
                var pkgType = (Event0x2DCSubType)messageEvent.MsgPush.CommonMessage.ContentHead.SubType;
                switch (pkgType)
                {
                    case Event0x2DCSubType.GroupGreyTipNotice20 when messageEvent.MsgPush.CommonMessage.MessageBody.MsgContent is { } content:
                    {
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
                    default:
                    {
                        context.LogWarning(nameof(PushLogic), "Unknown 0x2DC sub type: {0}", null, pkgType);
                        break;
                    }
                }
                break;
            }
            default:
            {
                context.LogWarning(nameof(PushLogic), "Unknown push msg type: {0}", null, messageEvent.MsgPush.CommonMessage.ContentHead.Type);
                break;
            }
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

    private enum DecreaseType
    {
        KickSelf = 3,
        Exit = 130,
        Kick = 131
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
    
    private enum Event0x210SubType
    {
        FriendRequestNotice = 35,
        FriendRecallNudgeNotice = 321,
    }
}
