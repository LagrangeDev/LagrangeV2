using System.Runtime.InteropServices;
using Lagrange.Core.Common.Entity;
using Lagrange.Core.Message.Entities;

namespace Lagrange.Core.NativeAPI.NativeModel.Message.Entity
{
    [StructLayout(LayoutKind.Sequential)]
    public struct ReplyEntityStruct
    {
        public ReplyEntityStruct() { }

        public ulong SrcUid;

        public ulong SrcSequence;

        public IntPtr Source;

        public int SourceType;

        public static implicit operator ReplyEntityStruct(ReplyEntity entity)
        {
            var type = entity.Source switch
            {
                BotFriend => 1,
                BotGroupMember => 2,
                BotStranger => 3,
                _ => 0
            };

            var sourcePtr = type switch
            {
                1 => Marshal.AllocHGlobal(Marshal.SizeOf<BotFriendStruct>()),
                2 => Marshal.AllocHGlobal(Marshal.SizeOf<BotGroupMemberStruct>()),
                3 => Marshal.AllocHGlobal(Marshal.SizeOf<BotStrangerStruct>()),
                _ => IntPtr.Zero
            };

            if (entity.Source != null && sourcePtr != 0)
            {
                switch (type)
                {
                    case 1: Marshal.StructureToPtr((BotFriendStruct)(BotFriend)entity.Source, sourcePtr, false); break;
                    case 2: Marshal.StructureToPtr((BotGroupMemberStruct)(BotGroupMember)entity.Source, sourcePtr, false); break;
                    case 3: Marshal.StructureToPtr((BotStrangerStruct)(BotStranger)entity.Source, sourcePtr, false); break;
                }
            }

            return new ReplyEntityStruct
            {
                SrcUid = entity.SrcUid,
                SrcSequence = entity.SrcSequence,
                Source = sourcePtr,
                SourceType = type
            };
        }
    }
}