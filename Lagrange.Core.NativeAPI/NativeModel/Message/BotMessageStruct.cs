using System.Runtime.InteropServices;
using System.Text;
using Lagrange.Core.Common.Entity;
using Lagrange.Core.Message;
using Lagrange.Core.Message.Entities;
using Lagrange.Core.NativeAPI.NativeModel.Common;
using Lagrange.Core.NativeAPI.NativeModel.Event;
using Lagrange.Core.NativeAPI.NativeModel.Message.Entity;

namespace Lagrange.Core.NativeAPI.NativeModel.Message
{
    [StructLayout(LayoutKind.Sequential)]
    public struct BotMessageStruct
    {
        public BotMessageStruct() { }

        //需要手动释放
        public IntPtr Contact = IntPtr.Zero;
        public IntPtr Receiver = IntPtr.Zero;

        public BotGroupStruct Group = new();

        public int Type = 0;

        public ByteArrayNative Time = new();

        public IntPtr Entities = IntPtr.Zero;

        public int EntityLength = 0;

        public static implicit operator BotMessageStruct(BotMessage message)
        {
            int type = 0;
            IntPtr contact = IntPtr.Zero;
            IntPtr receiver = IntPtr.Zero;
            switch (message.Type)
            {
                case MessageType.Group:
                    type = (int)MessageType.Group;
                    contact = Marshal.AllocHGlobal(Marshal.SizeOf<BotGroupMemberStruct>());
                    Marshal.StructureToPtr((BotGroupMemberStruct)(BotGroupMember)message.Contact, contact, false);

                    switch (message.Receiver)
                    {
                        case BotGroup group:
                            receiver = Marshal.AllocHGlobal(Marshal.SizeOf<BotGroupStruct>());
                            Marshal.StructureToPtr((BotGroupStruct)group, receiver, false);
                            break;
                        case BotGroupMember member:
                            receiver = Marshal.AllocHGlobal(Marshal.SizeOf<BotGroupMemberStruct>());
                            Marshal.StructureToPtr((BotGroupMemberStruct)member, receiver, false);
                            break;
                    }

                    break;
                case MessageType.Private:
                    type = (int)MessageType.Private;
                    contact = Marshal.AllocHGlobal(Marshal.SizeOf<BotFriendStruct>());
                    Marshal.StructureToPtr((BotFriendStruct)(BotFriend)message.Contact, contact, false);
                    receiver = Marshal.AllocHGlobal(Marshal.SizeOf<BotFriendStruct>());
                    Marshal.StructureToPtr((BotFriendStruct)(BotFriend)message.Receiver, receiver, false);
                    break;
                case MessageType.Temp:
                    type = (int)MessageType.Temp;
                    contact = Marshal.AllocHGlobal(Marshal.SizeOf<BotStrangerStruct>());
                    Marshal.StructureToPtr((BotStrangerStruct)(BotStranger)message.Contact, contact, false);
                    receiver = Marshal.AllocHGlobal(Marshal.SizeOf<BotStrangerStruct>());
                    Marshal.StructureToPtr((BotStrangerStruct)(BotStranger)message.Receiver, receiver, false);
                    break;
            }

            TypedEntityStruct[] entities = new TypedEntityStruct[message.Entities.Count];
            for (int i = 0; i < message.Entities.Count; i++)
            {
                switch (message.Entities[i])
                {
                    case ImageEntity imageEntity:
                        entities[i] = new TypedEntityStruct()
                        {
                            Type = (int)EntityType.ImageEntity,
                            Entity = Marshal.AllocHGlobal(Marshal.SizeOf<ImageEntityStruct>())
                        };
                        Marshal.StructureToPtr((ImageEntityStruct)imageEntity, entities[i].Entity, false);
                        break;
                    case MentionEntity mentionEntity:
                        entities[i] = new TypedEntityStruct()
                        {
                            Type = (int)EntityType.MentionEntity,
                            Entity = Marshal.AllocHGlobal(Marshal.SizeOf<MentionEntityStruct>())
                        };
                        Marshal.StructureToPtr((MentionEntityStruct)mentionEntity, entities[i].Entity, false);
                        break;
                    case RecordEntity recordEntity:
                        entities[i] = new TypedEntityStruct()
                        {
                            Type = (int)EntityType.RecordEntity,
                            Entity = Marshal.AllocHGlobal(Marshal.SizeOf<RecordEntityStruct>())
                        };
                        Marshal.StructureToPtr((RecordEntityStruct)recordEntity, entities[i].Entity, false);
                        break;
                    case ReplyEntity replyEntity:
                        entities[i] = new TypedEntityStruct()
                        {
                            Type = (int)EntityType.ReplyEntity,
                            Entity = Marshal.AllocHGlobal(Marshal.SizeOf<ReplyEntityStruct>())
                        };
                        Marshal.StructureToPtr((ReplyEntityStruct)replyEntity, entities[i].Entity, false);
                        break;
                    case TextEntity textEntity:
                        entities[i] = new TypedEntityStruct()
                        {
                            Type = (int)EntityType.TextEntity,
                            Entity = Marshal.AllocHGlobal(Marshal.SizeOf<TextEntityStruct>())
                        };
                        Marshal.StructureToPtr((TextEntityStruct)textEntity, entities[i].Entity, false);
                        break;
                    case VideoEntity videoEntity:
                        entities[i] = new TypedEntityStruct()
                        {
                            Type = (int)EntityType.VideoEntity,
                            Entity = Marshal.AllocHGlobal(Marshal.SizeOf<VideoEntityStruct>())
                        };
                        Marshal.StructureToPtr((VideoEntityStruct)videoEntity, entities[i].Entity, false);
                        break;
                    case MultiMsgEntity multiMsgEntity:
                        entities[i] = new TypedEntityStruct()
                        {
                            Type = (int)EntityType.MultiMsgEntity,
                            Entity = Marshal.AllocHGlobal(Marshal.SizeOf<MultiMsgEntityStruct>())
                        };
                        Marshal.StructureToPtr((MultiMsgEntityStruct)multiMsgEntity, entities[i].Entity, false);
                        break;
                    case GroupFileEntity groupFileEntity:
                        entities[i] = new TypedEntityStruct()
                        {
                            Type = (int)EntityType.GroupFileEntity,
                            Entity = Marshal.AllocHGlobal(Marshal.SizeOf<MultiMsgEntityStruct>())
                        };
                        Marshal.StructureToPtr((GroupFileEntityStruct)groupFileEntity, entities[i].Entity, false);
                        break;
                }
            }

            int entitiesLength = entities.Length;
            IntPtr entitiesPtr = Marshal.AllocHGlobal(Marshal.SizeOf<TypedEntityStruct>() * entitiesLength);
            for (int i = 0; i < entitiesLength; i++)
            {
                IntPtr entityPtr = entitiesPtr + i * Marshal.SizeOf<TypedEntityStruct>();
                Marshal.StructureToPtr(entities[i], entityPtr, false);
            }

            return new BotMessageStruct()
            {
                Contact = contact,
                Receiver = receiver,
                Group = message.Receiver switch
                {
                    BotGroup group => (BotGroupStruct)group,
                    BotGroupMember member => member.Group,
                    _ => new BotGroupStruct(),
                },
                Type = type,
                Time = Encoding.UTF8.GetBytes(message.Time.ToString("O")),
                Entities = entitiesPtr,
                EntityLength = entitiesLength
            };
        }
    }
}