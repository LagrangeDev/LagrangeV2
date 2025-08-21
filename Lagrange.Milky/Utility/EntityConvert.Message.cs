using Lagrange.Core.Common.Entity;
using Lagrange.Core.Message;
using Lagrange.Milky.Entity.Message;
using Lagrange.Milky.Extension;

namespace Lagrange.Milky.Utility;

public partial class EntityConvert
{
    public MessageBase MessageBase(BotMessage message) => message.Type switch
    {
        MessageType.Group => GroupMessage(message),
        MessageType.Private => FriendMessage(message),
        MessageType.Temp => TempMessage(message),
        _ => throw new NotSupportedException(),
    };

    public FriendMessage FriendMessage(BotMessage message) => new(
        message.Contact.Uin == _bot.BotUin ? message.Receiver.Uin : message.Contact.Uin,
        (long)message.ClientSequence,
        message.Contact.Uin,
        message.Time.ToUnixTimeSeconds(),
        Segments(message.Entities),
        Friend((BotFriend)message.Contact)
    );

    public GroupMessage GroupMessage(BotMessage message) => new(
        ((BotGroupMember)message.Contact).Group.Uin,
        (long)message.Sequence,
        message.Contact.Uin,
        message.Time.ToUnixTimeSeconds(),
        Segments(message.Entities),
        Group(((BotGroupMember)message.Contact).Group),
        GroupMember((BotGroupMember)message.Contact)
    );

    public TempMessage TempMessage(BotMessage message) => new(
        message.Contact.Uin == _bot.BotUin ? message.Receiver.Uin : message.Contact.Uin,
        (long)message.Sequence,
        message.Contact.Uin,
        message.Time.ToUnixTimeSeconds(),
        Segments(message.Entities),
        null
    );
}