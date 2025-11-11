using System.Text.Json;
using System.Web;
using Lagrange.Core.Common.Entity;
using Lagrange.Core.Events.EventArgs;
using Lagrange.Core.Internal.Events.Message;
using Lagrange.Core.Message;
using Lagrange.Core.Message.Entities;

namespace Lagrange.Core.Internal.Logic.Processors;

[MsgPushProcessor(MsgType.GroupMessage)]
[MsgPushProcessor(MsgType.PrivateMessage)]
[MsgPushProcessor(MsgType.TempMessage)]
internal class RichTextMsgProcessor : MsgPushProcessorBase
{
    internal override async ValueTask<bool> Handle(BotContext context, MsgType msgType, int subType, PushMessageEvent msgEvt, ReadOnlyMemory<byte>? content)
    {
        var message = await context.EventContext.GetLogic<MessagingLogic>().Parse(msgEvt.MsgPush.CommonMessage);

        if (message.Entities.Count > 0 && message.Entities[0] is LightAppEntity app && TryHandleLightApp(context, message, app)) return true;

        context.EventInvoker.PostEvent(new BotMessageEvent(message, msgEvt.Raw));
        return true;
    }

    private bool TryHandleLightApp(BotContext context, BotMessage message, LightAppEntity app)
    {
        if ((app.AppName == "com.tencent.qun.invite" || app.AppName == "com.tencent.qun.invite") && TryHandleQunInvite(context, message, app)) return true;

        return false;
    }

    private bool TryHandleQunInvite(BotContext context, BotMessage message, LightAppEntity app)
    {
        using var document = JsonDocument.Parse(app.Payload);
        var root = document.RootElement;

        string? bizsrc = root.GetProperty("bizsrc").GetString();
        if (app.AppName == "com.tencent.qun.invite" && bizsrc != "qun.invite") return false;

        string? url = root.GetProperty("meta").GetProperty("news").GetProperty("jumpUrl").GetString();
        if (url == null) throw new Exception($"TryHandleQunInvite failed! LightApp: {app.Payload}");

        var query = HttpUtility.ParseQueryString(new Uri(url).Query);
        long uin = uint.Parse(query["groupcode"] ?? throw new Exception($"TryHandleQunInvite failed! LightApp: {app.Payload}"));
        ulong sequence = ulong.Parse(query["msgseq"] ?? throw new Exception($"TryHandleQunInvite failed! LightApp: {app.Payload}"));

        context.EventInvoker.PostEvent(new BotGroupInviteNotificationEvent(new BotGroupInviteNotification(
            uin,
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
}