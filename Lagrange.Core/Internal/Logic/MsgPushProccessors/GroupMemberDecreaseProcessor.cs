using Lagrange.Core.Events.EventArgs;
using Lagrange.Core.Exceptions;
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
                    context.BotUin,
                    op.Operator.Uid != null ? (await context.CacheContext.ResolveStranger(op.Operator.Uid))?.Uin ?? 0 : null
                ));
                await context.CacheContext.RefreshGroups();
                return true;
            }
            case DecreaseType.Exit:
            {
                context.EventInvoker.PostEvent(new BotGroupMemberDecreaseEvent(
                    decrease.GroupUin,
                    (await context.CacheContext.ResolveStranger(decrease.MemberUid))?.Uin ?? 0,
                    null
                ));
                await context.CacheContext.RefreshGroupMembers(decrease.GroupUin);
                return true;
            }
            case DecreaseType.Kick:
            {
                var op = ProtoHelper.Deserialize<OperatorInfo>(decrease.Operator.AsSpan());
                context.EventInvoker.PostEvent(new BotGroupMemberDecreaseEvent(
                    decrease.GroupUin,
                    (await context.CacheContext.ResolveStranger(decrease.MemberUid))?.Uin ?? 0,
                    op.Operator.Uid != null ? (await context.CacheContext.ResolveStranger(op.Operator.Uid))?.Uin ?? 0 : null
                ));
                await context.CacheContext.RefreshGroupMembers(decrease.GroupUin);
                return true;
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