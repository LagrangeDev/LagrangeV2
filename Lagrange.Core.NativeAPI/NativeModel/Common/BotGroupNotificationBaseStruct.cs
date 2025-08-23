using System.Runtime.InteropServices;
using System.Text;
using Lagrange.Core.Common.Entity;

namespace Lagrange.Core.NativeAPI.NativeModel.Common;
[StructLayout(LayoutKind.Sequential)]
public abstract class BotGroupNotificationBaseStruct(BotGroupNotificationBase e)
{
    public long GroupUin = e.GroupUin;

    public ulong Sequence = e.Sequence;

    public int Type = (int)e.Type;

    public long TargetUin = e.TargetUin;

    public ByteArrayNative TargetUid = Encoding.UTF8.GetBytes(e.TargetUid);
}