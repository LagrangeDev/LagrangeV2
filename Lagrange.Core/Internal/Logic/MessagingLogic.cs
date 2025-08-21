using Lagrange.Core.Common.Entity;
using Lagrange.Core.Exceptions;
using Lagrange.Core.Internal.Events.Message;
using Lagrange.Core.Internal.Packets.Message;
using Lagrange.Core.Message;

namespace Lagrange.Core.Internal.Logic;

internal class MessagingLogic(BotContext context) : ILogic
{
    private readonly MessagePacker _packer = new(context);

    public Task<BotMessage> Parse(CommonMessage msg) => _packer.Parse(msg);

    public Task<CommonMessage> BuildFake(BotMessage msg) => _packer.BuildFake(msg);

    public async Task<List<BotMessage>> GetGroupMessage(long groupUin, ulong startSequence, ulong endSequence)
    {
        var result = await context.EventContext.SendEvent<GetGroupMessageEventResp>(new GetGroupMessageEventReq(groupUin, startSequence, endSequence));
        var messages = new List<BotMessage>(result.Chains.Count);
        foreach (var chain in result.Chains) messages.Add(await Parse(chain));
        return messages;
    }

    public async Task<List<BotMessage>> GetRoamMessage(long peerUin, uint time, uint count)
    {
        string peerUid = context.CacheContext.ResolveCachedUid(peerUin) ?? throw new InvalidTargetException(peerUin);
        var result = await context.EventContext.SendEvent<GetRoamMessageEventResp>(new GetRoamMessageEventReq(peerUid, time, count));
        var messages = new List<BotMessage>(result.Chains.Count);
        foreach (var chain in result.Chains) messages.Add(await Parse(chain));
        return messages;
    }
    
    public async Task<List<BotMessage>> GetC2CMessage(long peerUin, ulong startSequence, ulong endSequence)
    {
        string peerUid = context.CacheContext.ResolveCachedUid(peerUin) ?? throw new InvalidTargetException(peerUin);
        var result = await context.EventContext.SendEvent<GetC2CMessageEventResp>(new GetC2CMessageEventReq(peerUid, startSequence, endSequence));
        var messages = new List<BotMessage>(result.Chains.Count);
        foreach (var chain in result.Chains) messages.Add(await Parse(chain));
        return messages;
    }

    public async Task<BotMessage> SendFriendMessage(long friendUin, MessageChain chain)
    {
        var friend = await context.CacheContext.ResolveFriend(friendUin) ?? throw new InvalidTargetException(friendUin);
        var self = await context.CacheContext.ResolveFriend(context.BotUin) ?? throw new InvalidTargetException(context.BotUin);
        var message = await BuildMessage(chain, self, friend);
        var result = await context.EventContext.SendEvent<SendMessageEventResp>(new SendMessageEventReq(message));

        if (result == null) throw new InvalidOperationException();
        if (result.Result != 0) throw new OperationException(result.Result);

        message.Sequence = result.Sequence;
        message.Time = DateTimeOffset.FromUnixTimeSeconds(result.SendTime).DateTime;

        return message;
    }

    public async Task<BotMessage> SendGroupMessage(long groupUin, MessageChain chain)
    {
        var (group, self) = await context.CacheContext.ResolveMember(groupUin, context.BotUin) ?? throw new InvalidTargetException(context.BotUin, groupUin);
        var message = await BuildMessage(chain, self, group);
        var result = await context.EventContext.SendEvent<SendMessageEventResp>(new SendMessageEventReq(message));

        if (result == null) throw new InvalidOperationException();
        if (result.Result != 0) throw new OperationException(result.Result);

        message.Sequence = result.Sequence;
        message.Time = DateTimeOffset.FromUnixTimeSeconds(result.SendTime).DateTime;

        return message;
    }

    private async Task<BotMessage> BuildMessage(MessageChain chain, BotContact contact, BotContact receiver)
    {
        uint random = (uint)Random.Shared.Next();
        var message = new BotMessage(chain, contact, receiver, DateTime.Now)
        {
            Random = random,
            MessageId = (0x10000000ul << 32) | random
        };

        foreach (var entity in chain)
        {
            await entity.Preprocess(context, message);
        }

        return message;
    }
}