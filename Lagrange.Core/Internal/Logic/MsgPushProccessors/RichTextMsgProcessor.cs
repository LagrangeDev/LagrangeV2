using System.Text.Json;
using System.Web;
using Lagrange.Core.Common.Entity;
using Lagrange.Core.Events.EventArgs;
using Lagrange.Core.Internal.Events.Message;
using Lagrange.Core.Message.Entities;

namespace Lagrange.Core.Internal.Logic.MsgPushProccessors;

[MsgPushProcessor(MsgType.GroupMessage)]
[MsgPushProcessor(MsgType.PrivateMessage)]
[MsgPushProcessor(MsgType.TempMessage)]
internal class RichTextMsgProcessor : MsgPushProcessorBase
{
    internal override async ValueTask<bool> Handle(BotContext context, MsgType msgType, int subType, PushMessageEvent msgEvt, ReadOnlyMemory<byte>? content)
    {
        var message = await context.EventContext.GetLogic<MessagingLogic>().Parse(msgEvt.MsgPush.CommonMessage);
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
            return true;
        }
        context.EventInvoker.PostEvent(new BotMessageEvent(message, msgEvt.Raw));
        return true;
    }
}