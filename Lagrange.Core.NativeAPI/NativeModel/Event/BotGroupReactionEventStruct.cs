using System.Runtime.InteropServices;
using System.Text;
using Lagrange.Core.Events.EventArgs;
using Lagrange.Core.NativeAPI.NativeModel.Common;

namespace Lagrange.Core.NativeAPI.NativeModel.Event
{
    [StructLayout(LayoutKind.Sequential)]
    public struct BotGroupReactionEventStruct : IEventStruct
    {
        public BotGroupReactionEventStruct() { }

        public long TargetGroupUin = 0;

        public ulong TargetSequence = 0;

        public long OperatorUin = 0;

        public bool IsAdd = false;

        public ByteArrayNative Code = new();

        public ulong CurrentCount = 0;

        public static implicit operator BotGroupReactionEventStruct(BotGroupReactionEvent e)
        {
            return new BotGroupReactionEventStruct()
            {
                TargetGroupUin = e.TargetGroupUin,
                TargetSequence = e.TargetSequence,
                OperatorUin = e.OperatorUin,
                IsAdd = e.IsAdd,
                Code = Encoding.UTF8.GetBytes(e.Code),
                CurrentCount = e.CurrentCount
            };
        }
    }
}