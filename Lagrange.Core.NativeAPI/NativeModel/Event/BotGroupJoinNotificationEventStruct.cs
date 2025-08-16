using System.Runtime.InteropServices;
using Lagrange.Core.Common.Entity;
using Lagrange.Core.Events.EventArgs;

namespace Lagrange.Core.NativeAPI.NativeModel.Event
{
    [StructLayout(LayoutKind.Sequential)]
    public struct BotGroupJoinNotificationEventStruct(BotGroupJoinNotification notification) : IEventStruct
    {
        public BotGroupJoinNotification Notification = notification;

        //public static implicit operator BotGroupJoinNotificationEvent(BotGroupJoinNotificationEventStruct e)
        //{
        //    return new BotGroupJoinNotificationEvent(
        //        e.Notification
        //    );
        //}

        public static implicit operator BotGroupJoinNotificationEventStruct(BotGroupJoinNotificationEvent e)
        {
            return new BotGroupJoinNotificationEventStruct()
            {
                Notification = e.Notification
            };
        }
    }
}