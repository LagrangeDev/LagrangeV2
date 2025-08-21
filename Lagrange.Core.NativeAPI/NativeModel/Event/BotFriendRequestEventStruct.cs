using System.Runtime.InteropServices;
using System.Text;
using Lagrange.Core.Common.Entity;
using Lagrange.Core.Events.EventArgs;
using Lagrange.Core.NativeAPI.NativeModel.Common;

namespace Lagrange.Core.NativeAPI.NativeModel.Event
{
    [StructLayout(LayoutKind.Sequential)]
    public struct BotFriendRequestEventStruct : IEventStruct
    {
        public BotFriendRequestEventStruct() { }

        public ByteArrayNative InitiatorUid = new();

        public long InitiatorUin = 0;

        public ByteArrayNative Message = new();

        public ByteArrayNative Source = new();

        //public static implicit operator BotGroupMemberDecreaseEvent(BotGroupMemberDecreaseEventStruct e)
        //{
        //    return new BotGroupMemberDecreaseEvent(
        //        e.GroupUin, e.UserUin, e.OperatorUin
        //    );
        //}

        public static implicit operator BotFriendRequestEventStruct(BotFriendRequestEvent e)
        {
            return new BotFriendRequestEventStruct()
            {
                InitiatorUid = Encoding.UTF8.GetBytes(e.InitiatorUid),
                InitiatorUin = e.InitiatorUin,
                Message = Encoding.UTF8.GetBytes(e.Message),
                Source = Encoding.UTF8.GetBytes(e.Source)
            };
        }
    }
}