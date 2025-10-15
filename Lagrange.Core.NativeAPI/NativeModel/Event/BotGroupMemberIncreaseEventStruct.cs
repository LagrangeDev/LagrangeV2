using System.Runtime.InteropServices;
using Lagrange.Core.Events.EventArgs;

namespace Lagrange.Core.NativeAPI.NativeModel.Event
{
    [StructLayout(LayoutKind.Sequential)]
    public struct BotGroupMemberIncreaseEventStruct : IEventStruct
    {
        public BotGroupMemberIncreaseEventStruct() { }

        public long GroupUin = 0;

        public long MemberUin = 0;

        public long InvitorUin = 0;

        public uint Type = 0;

        public long OperatorUin = 0;

        public static implicit operator BotGroupMemberIncreaseEventStruct(BotGroupMemberIncreaseEvent e)
        {
            return new BotGroupMemberIncreaseEventStruct()
            {
                GroupUin = e.GroupUin,
                MemberUin = e.MemberUin,
                InvitorUin = e.InvitorUin,
                Type = e.Type,
                OperatorUin = e.OperatorUin ?? 0
            };
        }
    }
}