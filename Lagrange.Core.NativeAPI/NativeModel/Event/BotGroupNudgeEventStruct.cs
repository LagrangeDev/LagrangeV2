using System.Runtime.InteropServices;
using Lagrange.Core.Events.EventArgs;

namespace Lagrange.Core.NativeAPI.NativeModel.Event
{
    [StructLayout(LayoutKind.Sequential)]
    public struct BotGroupNudgeEventStruct : IEventStruct
    {
        public BotGroupNudgeEventStruct() { }
        public Int64 GroupUin = 0;
        public Int64 OperatorUin = 0;
        public Int64 TargetUin = 0;

        public static implicit operator BotGroupNudgeEvent(BotGroupNudgeEventStruct e)
        {
            return new BotGroupNudgeEvent(
                e.GroupUin, e.OperatorUin, e.TargetUin
            );
        }

        public static implicit operator BotGroupNudgeEventStruct(BotGroupNudgeEvent e)
        {
            return new BotGroupNudgeEventStruct()
            {
                GroupUin = e.GroupUin,
                OperatorUin = e.OperatorUin,
                TargetUin = e.TargetUin
            };
        }
    }
}