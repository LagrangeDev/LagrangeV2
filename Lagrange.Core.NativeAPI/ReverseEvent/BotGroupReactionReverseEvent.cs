using Lagrange.Core.Events.EventArgs;
using Lagrange.Core.NativeAPI.NativeModel;
using Lagrange.Core.NativeAPI.NativeModel.Event;
using Lagrange.Core.NativeAPI.ReverseEvent.Abstract;

namespace Lagrange.Core.NativeAPI.ReverseEvent
{
    public class BotGroupReactionReverseEvent : ReverseEventBase
    {
        public override void RegisterEventHandler(BotContext context)
        {
            context.EventInvoker.RegisterEvent<BotGroupReactionEvent>((ctx, e) =>
            {
                Events.Add((BotGroupReactionEventStruct)e);
            });
        }
    }
}