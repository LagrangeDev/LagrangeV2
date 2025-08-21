using System.Runtime.InteropServices;
using Lagrange.Core.Events.EventArgs;

namespace Lagrange.Core.NativeAPI.NativeModel.Event
{
    [StructLayout(LayoutKind.Sequential)]
    public struct BotGroupInviteSelfEventStruct : IEventStruct
    {
        public BotGroupInviteSelfEventStruct() { }

        public long InvitationSeq = 0;

        public long InitiatorUin = 0;

        public long GroupUin = 0;

        //public static implicit operator BotGroupMemberDecreaseEvent(BotGroupMemberDecreaseEventStruct e)
        //{
        //    return new BotGroupMemberDecreaseEvent(
        //        e.GroupUin, e.UserUin, e.OperatorUin
        //    );
        //}

        public static implicit operator BotGroupInviteSelfEventStruct(BotGroupInviteSelfEvent e)
        {
            return new BotGroupInviteSelfEventStruct()
            {
                InvitationSeq = e.InvitationSeq,
                InitiatorUin = e.InitiatorUin,
                GroupUin = e.GroupUin
            };
        }
    }
}