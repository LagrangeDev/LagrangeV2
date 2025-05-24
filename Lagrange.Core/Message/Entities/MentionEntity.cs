using Lagrange.Core.Internal.Packets.Message;
using Lagrange.Proto.Nodes;

namespace Lagrange.Core.Message.Entities;

public class MentionEntity(long uin, string display) : IMessageEntity
{
    public long Uin { get; } = uin;

    public string Display { get; } = display;
    
    internal string? Uid { get; private set; }
    
    public MentionEntity() : this(0, string.Empty) { }
    
    Task IMessageEntity.Preprocess(BotContext context, BotMessage message)
    {
        throw new NotImplementedException();
    }

    Elem[] IMessageEntity.Build()
    {
        throw new NotImplementedException();
    }
    
    IMessageEntity? IMessageEntity.Parse(List<Elem> elements, Elem target)
    {
        if (target.Text?.PbReserve is { Length: > 0 } reserve)
        {
            var obj = ProtoObject.Parse(reserve);
            if (!obj.ContainsKey(4)) return null; // fix handling super emoticon
            return new MentionEntity(obj[4].GetValue<long>(), target.Text.TextMsg)
            {
                Uid = obj[9].GetValue<string>()
            };
        }

        return null;
    }
}
