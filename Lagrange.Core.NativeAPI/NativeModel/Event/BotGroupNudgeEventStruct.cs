using System.Runtime.InteropServices;
using System.Text;
using Lagrange.Core.Events.EventArgs;
using Lagrange.Core.NativeAPI.NativeModel.Common;

namespace Lagrange.Core.NativeAPI.NativeModel.Event
{
    [StructLayout(LayoutKind.Sequential)]
    public struct BotGroupNudgeEventStruct : IEventStruct
    {
        public BotGroupNudgeEventStruct() { }

        public long GroupUin = 0;

        public long OperatorUin = 0;

        public ByteArrayNative Action = new();
        public long TargetUin = 0;

        public ByteArrayNative Suffix = new();

        public static implicit operator BotGroupNudgeEvent(BotGroupNudgeEventStruct e)
        {
            return new BotGroupNudgeEvent(
                e.GroupUin,
                e.OperatorUin,
                Encoding.UTF8.GetString(e.Action),
                e.TargetUin,
                Encoding.UTF8.GetString(e.Suffix)
            );
        }

        public static implicit operator BotGroupNudgeEventStruct(BotGroupNudgeEvent e)
        {
            return new BotGroupNudgeEventStruct()
            {
                GroupUin = e.GroupUin,
                OperatorUin = e.OperatorUin,
                Action = Encoding.UTF8.GetBytes(e.Action),
                TargetUin = e.TargetUin,
                Suffix = Encoding.UTF8.GetBytes(e.Suffix)
            };
        }
    }
}