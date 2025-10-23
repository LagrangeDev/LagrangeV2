using Lagrange.Core.Events.EventArgs;
using Lagrange.Core.Internal.Events.Message;
using Lagrange.Core.Internal.Packets.Notify;
using Lagrange.Core.Utility;

namespace Lagrange.Core.Internal.Logic.MsgPushProccessors;

[MsgPushProcessor(MsgType.Event0x210, 35, true)] // FriendRequestNotice
internal class FriendRequestProcessor : MsgPushProcessorBase
{
    internal override ValueTask<bool> Handle(BotContext context, MsgType msgType, int subType,
        PushMessageEvent msgEvt, ReadOnlyMemory<byte>? content)
    {
        var friendRequest = ProtoHelper.Deserialize<FriendRequest>(content!.Value.Span);
        context.EventInvoker.PostEvent(new BotFriendRequestEvent(
            friendRequest.Info!.SourceUid,
            msgEvt.MsgPush.CommonMessage.RoutingHead.FromUin,
            friendRequest.Info.Message,
            friendRequest.Info.Source ?? string.Empty
        ));
        return ValueTask.FromResult(true);
    }
}