using System.Text;
using Lagrange.Core.Events.EventArgs;
using Lagrange.Core.Internal.Events.Message;
using Lagrange.Core.Internal.Packets.Notify;
using Lagrange.Core.Utility;

namespace Lagrange.Core.Internal.Logic.Processors;

[MsgPushProcessor(MsgType.GroupMemberIncreaseNotice, true)]
internal class GroupMemberIncreaseProcessor : MsgPushProcessorBase
{
    internal override async ValueTask<bool> Handle(BotContext context, MsgType msgType, int subType, PushMessageEvent msgEvt, ReadOnlyMemory<byte>? content)
    {
        if (content == null) return false;

        var increase = ProtoHelper.Deserialize<GroupChange>(content.Value.Span);
        string? @operator = increase.Operator != null ? Encoding.UTF8.GetString(increase.Operator) : null;

        var @event = increase.Type switch
        {
            130 => new BotGroupMemberIncreaseEvent(
                increase.GroupUin,
                context.CacheContext.ResolveUin(increase.MemberUid),
                0,
                increase.Type,
                @operator != null ? context.CacheContext.ResolveUin(@operator) : 0
            ),
            131 => new BotGroupMemberIncreaseEvent(
                increase.GroupUin,
                context.CacheContext.ResolveUin(increase.MemberUid),
                @operator != null ? context.CacheContext.ResolveUin(@operator) : 0,
                increase.Type,
                0
            ),
            _ => null,
        };

        if (@event != null)
        {
            context.EventInvoker.PostEvent(@event);
            _ = await context.CacheContext.GetMemberList(increase.GroupUin, true);
            return true;
        }
        else return false;
    }
}