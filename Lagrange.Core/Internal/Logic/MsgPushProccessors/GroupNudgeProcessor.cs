using Lagrange.Core.Events.EventArgs;
using Lagrange.Core.Internal.Events.Message;
using Lagrange.Core.Internal.Packets.Notify;
using Lagrange.Core.Utility;
using Lagrange.Core.Utility.Binary;

namespace Lagrange.Core.Internal.Logic.MsgPushProccessors;

[MsgPushProcessor(MsgType.Event0x2DC, 20, true)] // GroupGreyTipNotice20
internal class GroupNudgeProcessor : MsgPushProcessorBase
{
    internal override ValueTask<bool> Handle(BotContext context, MsgType msgType, int subType,
        PushMessageEvent msgEvt, ReadOnlyMemory<byte>? content)
    {
        var packet = new BinaryPacket(content!.Value.Span);
        long groupUin = packet.Read<int>(); // group uin
        _ = packet.Read<byte>(); // unknown byte
        var proto = packet.ReadBytes(Prefix.Int16 | Prefix.LengthOnly);
        var greyTip = ProtoHelper.Deserialize<NotifyMessageBody>(proto);

        if (greyTip.SubType != 19) return ValueTask.FromResult(false); // GroupNudgeNotice

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
            return ValueTask.FromResult(true);
        }

        return ValueTask.FromResult(false);
    }
}