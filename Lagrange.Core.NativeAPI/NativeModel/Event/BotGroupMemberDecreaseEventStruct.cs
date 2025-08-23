using System.Runtime.InteropServices;
using Lagrange.Core.Events.EventArgs;

namespace Lagrange.Core.NativeAPI.NativeModel.Event
{
    [StructLayout(LayoutKind.Sequential)]
    public struct BotGroupMemberDecreaseEventStruct : IEventStruct
    {
        public BotGroupMemberDecreaseEventStruct() { }

        public long GroupUin = 0;

        public long UserUin = 0;

        public long OperatorUin = 0;

        public static implicit operator BotGroupMemberDecreaseEventStruct(BotGroupMemberDecreaseEvent e)
        {
            return new BotGroupMemberDecreaseEventStruct()
            {
                GroupUin = e.GroupUin,
                UserUin = e.UserUin,
                OperatorUin = e.OperatorUin ?? 0
            };
        }
    }
}