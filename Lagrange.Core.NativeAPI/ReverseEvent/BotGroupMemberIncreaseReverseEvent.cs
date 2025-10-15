using Lagrange.Core.Events.EventArgs;
using Lagrange.Core.NativeAPI.NativeModel.Event;
using Lagrange.Core.NativeAPI.ReverseEvent.Abstract;

namespace Lagrange.Core.NativeAPI.ReverseEvent
{
    public class BotGroupMemberIncreaseReverseEvent : ReverseEventBase
    {
        public override void RegisterEventHandler(BotContext context)
        {
            context.EventInvoker.RegisterEvent<BotGroupMemberIncreaseEvent>((ctx, e) =>
            {
                Events.Add((BotGroupMemberIncreaseEventStruct)e);
            });
        }
    }
}