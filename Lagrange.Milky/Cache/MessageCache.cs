using Lagrange.Core;
using Lagrange.Core.Common.Entity;
using Lagrange.Core.Common.Interface;
using Lagrange.Core.Events.EventArgs;
using Lagrange.Core.Message;
using Lagrange.Milky.Configuration;
using Lagrange.Milky.Utility.Cache;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Lagrange.Milky.Cache;

public class MessageCache(BotContext bot, IOptions<MilkyConfiguration> options) : IHostedService
{
    private readonly BotContext _bot = bot;

    public readonly string _policy = options.Value.Message.Cache.Policy;

    private readonly ICache<MessageKey, BotMessage> _cache = options.Value.Message.Cache.Policy switch
    {
        "LRU" => new LruCache<MessageKey, BotMessage>(options.Value.Message.Cache.Capacity),
        "FIFO" => new FifoCache<MessageKey, BotMessage>(options.Value.Message.Cache.Capacity),
        _ => throw new NotSupportedException(),
    };

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _bot.EventInvoker.RegisterEvent<BotMessageEvent>(HandleBotMessageEvent);

        return Task.CompletedTask;
    }

    private void HandleBotMessageEvent(BotContext bot, BotMessageEvent @event)
    {
        var message = @event.Message;

        MessageType type = message.Type;
        long peer = message.Type switch
        {
            MessageType.Group => ((BotGroupMember)message.Contact).Group.Uin,
            MessageType.Private => message.Contact.Uin == bot.BotUin ? message.Receiver.Uin : message.Contact.Uin,
            MessageType.Temp => throw new NotSupportedException(),
            _ => throw new NotSupportedException(),
        };
        ulong sequence = message.Type == MessageType.Private ? message.ClientSequence : message.Sequence;

        _cache.Put(new MessageKey(type, peer, sequence), message);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        // TODO
        // _bot.EventInvoker.UnregisterEvent<BotMessageEvent>(HandleBotMessageEvent);

        return Task.CompletedTask;
    }

    public async Task<BotMessage?> GetMessageAsync(MessageType type, long peer, ulong sequence, CancellationToken token)
    {
        var key = new MessageKey(type, peer, sequence);

        var message = _cache.Get(key);
        if (message == null)
        {
            var messages = type switch
            {
                MessageType.Group => await _bot.GetGroupMessage(peer, sequence, sequence).WaitAsync(token),
                MessageType.Private => await _bot.GetC2CMessage(peer, sequence, sequence).WaitAsync(token),
                MessageType.Temp => throw new NotSupportedException(),
                _ => throw new NotSupportedException(),
            };
            if (messages.Count == 0) return null;

            message = messages[0];
        }

        if (_policy == "LRU") _cache.Put(key, message);

        return message;
    }

    public class MessageKey(MessageType type, long peer, ulong sequence)
    {
        public MessageType Type { get; } = type;

        public long Peer { get; } = peer;

        public ulong Sequence { get; } = sequence;

        public override bool Equals(object? obj)
        {
            return obj is MessageKey other
                && Type == other.Type
                && Peer == other.Peer
                && Sequence == other.Sequence;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Type, Peer, Sequence);
        }
    }
}