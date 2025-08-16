using System.Runtime.InteropServices;
using Lagrange.Core.Events.EventArgs;

namespace Lagrange.Core.NativeAPI.NativeModel.Event
{
    [StructLayout(LayoutKind.Sequential)]
    public struct BotGroupMemberDecreaseEventStruct : IEventStruct
    {
        public BotGroupMemberDecreaseEventStruct() { }
        public Int64 GroupUin = 0;
        public Int64 UserUin = 0;
        public Int64 OperatorUin = 0;

        //public static implicit operator BotGroupMemberDecreaseEvent(BotGroupMemberDecreaseEventStruct e)
        //{
        //    return new BotGroupMemberDecreaseEvent(
        //        e.GroupUin, e.UserUin, e.OperatorUin
        //    );
        //}

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