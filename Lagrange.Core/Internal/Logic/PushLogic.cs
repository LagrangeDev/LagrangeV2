using System.Text;
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
                if (message.Entities[0] is LightAppEntity { AppName: "com.tencent.qun.invite" } || message.Entities[0] is LightAppEntity { AppName: "com.tencent.tuwen.lua" })
                {
                    var app = (LightAppEntity)message.Entities[0];
                    using var document = JsonDocument.Parse(app.Payload);
                    var root = document.RootElement;

                    string url = root.GetProperty("meta").GetProperty("news").GetProperty("jumpUrl").GetString() ?? throw new Exception("sb tx! Is this 'com.tencent.qun.invite' or 'com.tencent.tuwen.lua'?");
                    var query = HttpUtility.ParseQueryString(new Uri(url).Query);
                    long groupUin = uint.Parse(query["groupcode"] ?? throw new Exception("sb tx! Is this '/group/invite_join'?"));
                    ulong sequence = ulong.Parse(query["msgseq"] ?? throw new Exception("sb tx! Is this '/group/invite_join'?"));
                    context.EventInvoker.PostEvent(new BotGroupInviteNotificationEvent(new BotGroupInviteNotification(
                        groupUin,
                        sequence,
                        context.BotUin,
                        context.CacheContext.ResolveCachedUid(context.BotUin) ?? string.Empty,
                        BotGroupNotificationState.Wait,
                        null,
                        null,
                        message.Contact.Uin,
                        message.Contact.Uid,
                        false
                    )));
                    break;
                }
                context.EventInvoker.PostEvent(new BotMessageEvent(message, messageEvent.Raw));
                break;
            }
            case Type.GroupMemberIncreaseNotice when messageEvent.MsgPush.CommonMessage.MessageBody.MsgContent is { } content:
            {
                var increase = ProtoHelper.Deserialize<GroupChange>(content.Span);
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
                    context.CacheContext.ResolveUin(increase.MemberUid),
                    context.CacheContext.ResolveUin(Encoding.UTF8.GetString(increase.Operator.AsSpan())),
                    increase.IncreaseType,
                    operatorUin));
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
                        context.LogDebug(nameof(PushLogic), "Unknown decrease type: {0}", null, decrease.DecreaseType);
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
                        context.LogDebug(nameof(PushLogic), "Unknown 0x20D sub type: {0}", null, @event.SubType);
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
                    case Event0x210SubType.FriendRequestNotice when messageEvent.MsgPush.CommonMessage.MessageBody.MsgContent is { } content:
                    {
                        var friendRequest = ProtoHelper.Deserialize<FriendRequest>(content.Span);
                        context.EventInvoker.PostEvent(new BotFriendRequestEvent(
                            friendRequest.Info!.SourceUid,
                            messageEvent.MsgPush.CommonMessage.RoutingHead.FromUin,
                            friendRequest.Info.Message,
                            friendRequest.Info.Source ?? string.Empty
                        ));
                        break;
                    }
                    default:
                    {
                        context.LogDebug(nameof(PushLogic), "Unknown 0x210 sub type: {0}", null, pkgType210);
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
                    {
                        var packet = new BinaryPacket(content);
                        long groupUin = packet.Read<int>(); // group uin
                        _ = packet.Read<byte>(); // unknown byte
                        var proto = packet.ReadBytes(Prefix.Int16 | Prefix.LengthOnly);
                        var greyTip = ProtoHelper.Deserialize<NotifyMessageBody>(proto);

                        switch ((Event0x2DCSubType20SubType)greyTip.SubType)
                        {
                            case Event0x2DCSubType20SubType.GroupNudgeNotice:
                            {
                                var @params = greyTip.GeneralGrayTip.MsgTemplParam.ToDictionary(x => x.Name, x => x.Value);

                                if (greyTip.GeneralGrayTip.BusiType == 12) // poke
                                {
                                    context.EventInvoker.PostEvent(new BotGroupNudgeEvent(
                                        groupUin,
                                        long.Parse(@params["uin_str1"]),
                                        @params["action_str"],
                                        @params["action_img_url"],
                                        long.Parse(@params["uin_str2"]),
                                        @params["suffix_str"]
                                    ));
                                }
                                break;
                            }
                        }
                        break;
                    }
                    case Event0x2DCSubType.SubType16 when messageEvent.MsgPush.CommonMessage.MessageBody.MsgContent is { } content:
                    {
                        var reader = new BinaryPacket(content);
                        // group uin and 1 byte
                        reader.Skip(4 + 1);
                        var proto = reader.ReadBytes(Prefix.Int16 | Prefix.LengthOnly);
                        var body = ProtoHelper.Deserialize<NotifyMessageBody>(proto);

                        switch ((Event0x2DCSubType16SubType)body.SubType)
                        {
                            case Event0x2DCSubType16SubType.GroupReactionNotice:
                            {
                                var reaction = body.Reaction.Data.Data;

                                long @operator = context.CacheContext.ResolveUin(reaction.Data.OperatorUid);

                                context.EventInvoker.PostEvent(new BotGroupReactionEvent(
                                    body.GroupUin,
                                    reaction.Target.Sequence,
                                    @operator,
                                    reaction.Data.Type == 1,
                                    reaction.Data.Code,
                                    reaction.Data.CurrentCount
                                ));
                                break;
                            }
                            default:
                            {
                                context.LogDebug(nameof(PushLogic), "Unknown 0x2DCSub16 sub type: {0}", null, body.SubType);
                                break;
                            }
                        }
                        break;
                    }
                    default:
                    {
                        context.LogDebug(nameof(PushLogic), "Unknown 0x2DC sub type: {0}", null, pkgType);
                        break;
                    }
                }
                break;
            }
            default:
            {
                context.LogDebug(nameof(PushLogic), "Unknown push msg type: {0}", null, messageEvent.MsgPush.CommonMessage.ContentHead.Type);
                break;
            }
        }
    }

    private enum Type
    {
        GroupMemberIncreaseNotice = 33,
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

    private enum Event0x2DCSubType16SubType
    {
        GroupReactionNotice = 35,
    }

    private enum Event0x2DCSubType20SubType
    {
        GroupNudgeNotice = 19,
    }

    private enum Event0x210SubType
    {
        FriendRequestNotice = 35,
        FriendRecallNudgeNotice = 321,
    }
}
