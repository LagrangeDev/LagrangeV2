using Lagrange.Core.Events.EventArgs;
using Lagrange.Core.Internal.Events.Message;
using Lagrange.Core.Internal.Packets.Notify;
using Lagrange.Core.Utility;
using Lagrange.Core.Utility.Binary;

namespace Lagrange.Core.Internal.Logic.MsgPushProccessors;

[MsgPushProcessor(MsgType.Event0x2DC, 17, true)]
internal class GroupRecallMessageProcessor : MsgPushProcessorBase
{
    internal override async ValueTask<bool> Handle(BotContext context, MsgType msgType, int subType, PushMessageEvent msgEvt, ReadOnlyMemory<byte>? content)
    {
        var packet = new BinaryPacket(content!.Value.Span);
        // group uin and 1 byte
        packet.Skip(4 + 1);

        var notify = ProtoHelper.Deserialize<NotifyMessageBody>(packet.ReadBytes(Prefix.Int16 | Prefix.LengthOnly));
        foreach (var message in notify.Recall.RecallMessages)
        {
            var @event = new BotGroupRecallEvent(
                notify.GroupUin,
                message.Sequence,
                (await context.CacheContext.ResolveGroupMember(notify.GroupUin, message.AuthorUid))?.Uin ?? 0,
                notify.Recall.OperatorUid != null ? (await context.CacheContext.ResolveGroupMember(notify.GroupUin, notify.Recall.OperatorUid))?.Uin ?? 0 : 0,
                notify.Recall.TipInfo?.Tip ?? string.Empty
            );
            context.EventInvoker.PostEvent(@event);
        }

        return true;
    }
}