using System.Runtime.InteropServices;
using System.Text;
using Lagrange.Core.Common.Entity;

namespace Lagrange.Core.NativeAPI.NativeModel.Common;
[StructLayout(LayoutKind.Sequential)]
public class BotGroupInviteNotificationStruct(BotGroupInviteNotification e) : BotGroupNotificationBaseStruct(e)
{
    public int State = (int)e.State;

    public long OperatorUin = e.OperatorUin ?? 0;

    public ByteArrayNative OperatorUid = Encoding.UTF8.GetBytes(e.OperatorUid ?? "");

    public long InviterUin = e.InviterUin;

    public ByteArrayNative InviterUid = Encoding.UTF8.GetBytes(e.InviterUid);

    public bool IsFiltered = e.IsFiltered;

    public static implicit operator BotGroupInviteNotificationStruct(BotGroupInviteNotification e)
    {
        return new BotGroupInviteNotificationStruct(e);
    }
}
