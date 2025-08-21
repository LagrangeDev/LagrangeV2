using Lagrange.Core.Events.EventArgs;
using Lagrange.Core.NativeAPI.NativeModel;
using Lagrange.Core.NativeAPI.NativeModel.Event;
using Lagrange.Core.NativeAPI.ReverseEvent.Abstract;

namespace Lagrange.Core.NativeAPI.ReverseEvent
{
    public class BotFriendRequestReverseEvent : ReverseEventBase
    {
        public override void RegisterEventHandler(BotContext context)
        {
            context.EventInvoker.RegisterEvent<BotFriendRequestEvent>((ctx, e) =>
            {
                Events.Add((BotFriendRequestEventStruct)e);
            });
        }
    }
}