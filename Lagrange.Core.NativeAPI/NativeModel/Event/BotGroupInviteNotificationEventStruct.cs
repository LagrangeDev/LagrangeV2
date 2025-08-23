using System.Runtime.InteropServices;
using Lagrange.Core.Common.Entity;
using Lagrange.Core.Events.EventArgs;
using Lagrange.Core.NativeAPI.NativeModel.Common;

namespace Lagrange.Core.NativeAPI.NativeModel.Event;
[StructLayout(LayoutKind.Sequential)]
public struct BotGroupInviteNotificationEventStruct(BotGroupInviteNotification notification) : IEventStruct
{
    public BotGroupInviteNotificationStruct Notification = notification;

    public static implicit operator BotGroupInviteNotificationEventStruct(BotGroupInviteNotificationEvent e)
    {
        return new BotGroupInviteNotificationEventStruct(e.Notification);
    }
}
