using Lagrange.Core.Events.EventArgs;
using Lagrange.Core.Internal.Events.Message;
using Lagrange.Core.Internal.Packets.Notify;
using Lagrange.Core.Utility;
using Lagrange.Core.Utility.Binary;

namespace Lagrange.Core.Internal.Logic.MsgPushProccessors;

[MsgPushProcessor(MsgType.Event0x2DC, 16, true)]
internal class GroupReactionProcessor : MsgPushProcessorBase
{
    internal override ValueTask<bool> Handle(BotContext context, MsgType msgType, int subType,
        PushMessageEvent msgEvt, ReadOnlyMemory<byte>? content)
    {
        var reader = new BinaryPacket(content!.Value.Span);
        // group uin and 1 byte
        reader.Skip(4 + 1);
        var proto = reader.ReadBytes(Prefix.Int16 | Prefix.LengthOnly);
        var body = ProtoHelper.Deserialize<NotifyMessageBody>(proto);
        if (body.SubType != 35) return ValueTask.FromResult(false); // GroupReactionNotice
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
        return ValueTask.FromResult(true);
    }
}