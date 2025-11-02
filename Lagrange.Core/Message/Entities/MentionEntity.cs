using Lagrange.Core.Common.Entity;
using Lagrange.Core.Exceptions;
using Lagrange.Core.Internal.Packets.Message;
using Lagrange.Core.Utility.Binary;
using Lagrange.Core.Utility;

namespace Lagrange.Core.Message.Entities;

public class MentionEntity(long uin, string? display) : IMessageEntity
{
    public long Uin { get; } = uin;

    public string? Display { get; private set; } = display;
    
    internal string? Uid { get; private set; }
    
    public MentionEntity() : this(0, null) { }

    async Task IMessageEntity.Preprocess(BotContext context, BotMessage message)
    {
        if (Uin != 0)
        {
            BotContact contact;
            switch (message.Receiver)
            {
                case BotGroup group: 
                    contact = await context.CacheContext.ResolveGroupMember(group.Uin, Uin) ?? throw new InvalidTargetException(Uin, group.Uin);
                    break;
                case BotFriend: // Maybe self
                    contact = await context.CacheContext.ResolveFriend(Uin) ?? throw new InvalidTargetException(Uin);
                    break;
                case BotStranger stranger: // hummmm...
                    contact = stranger;
                    break;
                default:
                    throw new InvalidTargetException(Uin, message.Contact.Uin);
            }

            Display ??= contact.Nickname;
            if (!Display.StartsWith('@')) Display = '@' + Display;
            Uid = contact.Uid;
        }
    }
    
    string IMessageEntity.ToPreviewString() => Display ?? "";

    Elem[] IMessageEntity.Build()
    {
        var resvAttr = new TextResvAttr
        {
            AtType = Uin == 0 ? 1u : 2u, // 1 for mention all
            AtMemberUin = (ulong)Uin,
            AtMemberTinyid = 0,
            AtMemberUid = Uid
        };
        
        return
        [
            new Elem
            {
                Text = new Text
                {
                    TextMsg = Display ?? throw new InvalidOperationException("Display cannot be null"),
                    PbReserve = ProtoHelper.Serialize(resvAttr),
                }
            }
        ];
    }
    
    IMessageEntity? IMessageEntity.Parse(List<Elem> elements, Elem target)
    {
        if (target.Text?.Attr6Buf is { Length: > 0 } attr6Buf)
        {
            var reader = new BinaryPacket(attr6Buf.AsSpan());
            reader.Skip(2);
            
            short startPos = reader.Read<short>();
            short textLen = reader.Read<short>();
            byte flag = reader.Read<byte>();
            uint uin = reader.Read<uint>();
            ushort wExtBufLen = reader.Read<ushort>();
            
            return new MentionEntity(uin, target.Text.TextMsg);
        }

        return null;
    }
}
