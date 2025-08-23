using System.Runtime.InteropServices;
using System.Text;
using Lagrange.Core.Common.Entity;

namespace Lagrange.Core.NativeAPI.NativeModel.Common;
[StructLayout(LayoutKind.Sequential)]
public class BotGroupJoinNotificationStruct(BotGroupJoinNotification notification) : BotGroupNotificationBaseStruct(notification)
{
    public int State = (int)notification.State;

    public long OperatorUin = notification.OperatorUin ?? 0;

    public ByteArrayNative OperatorUid { get; } = Encoding.UTF8.GetBytes(notification.OperatorUid ?? "");

    public ByteArrayNative Comment { get; } = Encoding.UTF8.GetBytes(notification.Comment);

    public bool IsFiltered { get; } = notification.IsFiltered;

    public static implicit operator BotGroupJoinNotificationStruct(BotGroupJoinNotification e)
    {
        return new BotGroupJoinNotificationStruct(e);
    }
}
