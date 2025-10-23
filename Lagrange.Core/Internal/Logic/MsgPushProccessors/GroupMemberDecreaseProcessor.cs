using Lagrange.Core.Events.EventArgs;
using Lagrange.Core.Internal.Events.Message;
using Lagrange.Core.Internal.Packets.Notify;
using Lagrange.Core.Utility;

namespace Lagrange.Core.Internal.Logic.MsgPushProccessors;

[MsgPushProcessor(MsgType.GroupMemberDecreaseNotice, true)]
internal class GroupMemberDecreaseProcessor : MsgPushProcessorBase
{
    internal override async ValueTask<bool> Handle(BotContext context, MsgType msgType, int subType, PushMessageEvent msgEvt, ReadOnlyMemory<byte>? content)
    {
        var decrease = ProtoHelper.Deserialize<GroupChange>(content!.Value.Span);
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
                return true;
            }
            case DecreaseType.Exit:
            {
                await context.CacheContext.GetMemberList(decrease.GroupUin);
                context.EventInvoker.PostEvent(new BotGroupMemberDecreaseEvent(
                    decrease.GroupUin,
                    context.CacheContext.ResolveUin(decrease.MemberUid),
                    null
                ));
                return true;
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

        return false;
    }
    
    private enum DecreaseType
    {
        KickSelf = 3,
        Exit = 130,
        Kick = 131
    }
}