using System.Runtime.InteropServices;
using Lagrange.Core.Common.Entity;
using Lagrange.Core.Events.EventArgs;
using Lagrange.Core.NativeAPI.NativeModel.Common;

namespace Lagrange.Core.NativeAPI.NativeModel.Event
{
    [StructLayout(LayoutKind.Sequential)]
    public struct BotGroupJoinNotificationEventStruct(BotGroupJoinNotification notification) : IEventStruct
    {
        public BotGroupJoinNotificationStruct Notification = notification;

        public static implicit operator BotGroupJoinNotificationEventStruct(BotGroupJoinNotificationEvent e)
        {
            return new BotGroupJoinNotificationEventStruct(e.Notification);
        }
    }
}