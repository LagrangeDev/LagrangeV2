using Lagrange.Core.Events.EventArgs;
using Lagrange.Core.Internal.Events.Message;
using Lagrange.Core.Internal.Packets.Notify;
using Lagrange.Core.Utility;

namespace Lagrange.Core.Internal.Logic.Processors;

[MsgPushProcessor(MsgType.Event0x210, 138, true)]
[MsgPushProcessor(MsgType.Event0x210, 139, true)]
internal class FriendRecallMessageProcessor : MsgPushProcessorBase
{
    internal override ValueTask<bool> Handle(BotContext bot, MsgType msgType, int subType, PushMessageEvent msgEvt, ReadOnlyMemory<byte>? content)
    {
        var recall = ProtoHelper.Deserialize<FriendRecall>(content!.Value.Span);

        long fromUin = bot.CacheContext.ResolveUin(recall.Info.FromUid);
        long toUin = bot.CacheContext.ResolveUin(recall.Info.ToUid);
        var @event = new BotFriendRecallEvent(
                fromUin == bot.BotUin ? toUin : fromUin,
                fromUin,
                (ulong)recall.Info.Sequence,
                recall.Info.TipInfo?.Tip ?? string.Empty
            );
            bot.EventInvoker.PostEvent(@event);

        return ValueTask.FromResult(true);
    }
}