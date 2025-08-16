using System.Runtime.InteropServices;
using Lagrange.Core.Common.Entity;
using Lagrange.Core.Events.EventArgs;

namespace Lagrange.Core.NativeAPI.NativeModel.Event
{
    [StructLayout(LayoutKind.Sequential)]
    public struct BotGroupInviteNotificationEventStruct(BotGroupInviteNotification notification) : IEventStruct
    {
        public BotGroupInviteNotification Notification = notification;

        //public static implicit operator BotGroupMemberDecreaseEvent(BotGroupMemberDecreaseEventStruct e)
        //{
        //    return new BotGroupMemberDecreaseEvent(
        //        e.GroupUin, e.UserUin, e.OperatorUin
        //    );
        //}

        public static implicit operator BotGroupInviteNotificationEventStruct(BotGroupInviteNotificationEvent e)
        {
            return new BotGroupInviteNotificationEventStruct()
            {
                Notification = e.Notification
            };
        }
    }
}