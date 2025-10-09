using Lagrange.Core;
using Lagrange.Core.Common.Entity;
using Lagrange.Core.Message;
using Lagrange.Milky.Configuration;
using Lagrange.Milky.Extension;
using Lagrange.Milky.Utility;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using static Lagrange.Core.Events.EventArgs.BotOfflineEvent;
using LgrEvents = Lagrange.Core.Events.EventArgs;

namespace Lagrange.Milky.Event;

public class EventService(ILogger<EventService> logger, IOptions<MilkyConfiguration> options, BotContext bot, EntityConvert convert) : IHostedService
{
    private readonly ILogger<EventService> _logger = logger;

    private readonly bool _ignoreBotMessage = options.Value.Message.IgnoreBotMessage;

    private readonly BotContext _bot = bot;
    private readonly EntityConvert _convert = convert;

    private readonly HashSet<Action<Memory<byte>>> _handlers = [];
    private readonly ReaderWriterLockSlim _lock = new();

    public Task StartAsync(CancellationToken token)
    {
        _bot.EventInvoker.RegisterEvent<LgrEvents.BotOfflineEvent>(HandleOfflineEvent);
        _bot.EventInvoker.RegisterEvent<LgrEvents.BotMessageEvent>(HandleMessageEvent);
        _bot.EventInvoker.RegisterEvent<LgrEvents.BotGroupNudgeEvent>(HandleGroupNudgeEvent);
        _bot.EventInvoker.RegisterEvent<LgrEvents.BotGroupMemberIncreaseEvent>(HandleGroupMemberIncreaseEvent);
        _bot.EventInvoker.RegisterEvent<LgrEvents.BotGroupMemberDecreaseEvent>(HandleGroupMemberDecreaseEvent);
        _bot.EventInvoker.RegisterEvent<LgrEvents.BotFriendRequestEvent>(HandleFriendRequestEvent);

        return Task.CompletedTask;
    }

    private void HandleOfflineEvent(BotContext bot, LgrEvents.BotOfflineEvent @event)
    {
        try
        {
            _logger.LogOffline(@event.Reason, @event.Tips?.Tag, @event.Tips?.Message);

            var result = _convert.BotOfflineEvent(@event);
            byte[] bytes = JsonUtility.SerializeToUtf8Bytes(result.GetType(), result);
            using (_lock.UsingReadLock())
            {
                foreach (var handler in _handlers)
                {
                    handler(bytes);
                }
            }
        }
        catch (Exception e)
        {
            _logger.LogHandleEventException(nameof(LgrEvents.BotOfflineEvent), e);
        }
    }

    private void HandleMessageEvent(BotContext bot, LgrEvents.BotMessageEvent @event)
    {
        try
        {
            switch (@event.Message.Type)
            {
                case MessageType.Group:
                    _logger.LogGroupMessage(
                        @event.Message.Type,
                        ((BotGroupMember)@event.Message.Contact).Group.Uin,
                        @event.Message.Contact.Uin,
                        @event.Message.Entities.ToDebugString()
                    );
                    break;
                case MessageType.Private:
                case MessageType.Temp:
                    _logger.LogPrivateMessage(
                        @event.Message.Type,
                        @event.Message.Contact.Uin,
                        @event.Message.Entities.ToDebugString()
                    );
                    break;
            }

            if (_ignoreBotMessage && @event.Message.Contact.Uin == bot.BotUin) return;

            var result = _convert.MessageReceiveEvent(@event);
            byte[] bytes = JsonUtility.SerializeToUtf8Bytes(result.GetType(), result);
            using (_lock.UsingReadLock())
            {
                foreach (var handler in _handlers)
                {
                    handler(bytes);
                }
            }
        }
        catch (Exception e)
        {
            _logger.LogHandleEventException(nameof(LgrEvents.BotMessageEvent), e);
        }
    }

    private void HandleGroupNudgeEvent(BotContext bot, LgrEvents.BotGroupNudgeEvent @event)
    {
        try
        {
            _logger.LogGroupNudgeEvent(
                @event.GroupUin,
                @event.OperatorUin,
                @event.TargetUin
            );
            var result = _convert.GroupNudgeEvent(@event);
            byte[] bytes = JsonUtility.SerializeToUtf8Bytes(result.GetType(), result);
            using (_lock.UsingReadLock())
            {
                foreach (var handler in _handlers)
                {
                    handler(bytes);
                }
            }
        }
        catch (Exception e)
        {
            _logger.LogHandleEventException(nameof(LgrEvents.BotGroupNudgeEvent), e);
        }
    }

    private void HandleGroupMemberIncreaseEvent(BotContext bot, LgrEvents.BotGroupMemberIncreaseEvent @event)
    {
        try
        {
            _logger.LogGroupMemberIncreaseEvent(
                @event.GroupUin,
                @event.MemberUin,
                @event.OperatorUin,
                @event.InvitorUin
            );
            var result = _convert.GroupMemberIncreaseEvent(@event);
            byte[] bytes = JsonUtility.SerializeToUtf8Bytes(result.GetType(), result);
            using (_lock.UsingReadLock())
            {
                foreach (var handler in _handlers)
                {
                    handler(bytes);
                }
            }
        }
        catch (Exception e)
        {
            _logger.LogHandleEventException(nameof(LgrEvents.BotGroupMemberIncreaseEvent), e);
        }
    }
    
    private void HandleGroupMemberDecreaseEvent(BotContext bot, LgrEvents.BotGroupMemberDecreaseEvent @event)
    {
        try
        {
            _logger.LogGroupMemberDecreaseEvent(
                @event.GroupUin,
                @event.UserUin,
                @event.OperatorUin
            );
            var result = _convert.GroupMemberDecreaseEvent(@event);
            byte[] bytes = JsonUtility.SerializeToUtf8Bytes(result.GetType(), result);
            using (_lock.UsingReadLock())
            {
                foreach (var handler in _handlers)
                {
                    handler(bytes);
                }
            }
        }
        catch (Exception e)
        {
            _logger.LogHandleEventException(nameof(LgrEvents.BotGroupMemberDecreaseEvent), e);
        }
    }

    private void HandleFriendRequestEvent(BotContext bot, LgrEvents.BotFriendRequestEvent @event)
    {
        try
        {
            _logger.LogBotFriendRequestEvent(
                @event.InitiatorUid,
                @event.InitiatorUin,
                @event.Message,
                @event.Source
            );
            var result = _convert.FriendRequestEvent(@event);
            byte[] bytes = JsonUtility.SerializeToUtf8Bytes(result.GetType(), result);
            using (_lock.UsingReadLock())
            {
                foreach (var handler in _handlers)
                {
                    handler(bytes);
                }
            }
        }
        catch (Exception e)
        {
            _logger.LogHandleEventException(nameof(LgrEvents.BotFriendRequestEvent), e);
        }
    }

    public Task StopAsync(CancellationToken token)
    {
        _bot.EventInvoker.UnregisterEvent<LgrEvents.BotMessageEvent>(HandleMessageEvent);
        _bot.EventInvoker.UnregisterEvent<LgrEvents.BotMessageEvent>(HandleMessageEvent);
        _bot.EventInvoker.UnregisterEvent<LgrEvents.BotGroupNudgeEvent>(HandleGroupNudgeEvent);
        _bot.EventInvoker.UnregisterEvent<LgrEvents.BotGroupMemberDecreaseEvent>(HandleGroupMemberDecreaseEvent);
        _bot.EventInvoker.UnregisterEvent<LgrEvents.BotFriendRequestEvent>(HandleFriendRequestEvent);

        return Task.CompletedTask;
    }

    public void Register(Action<Memory<byte>> handler)
    {
        using (_lock.UsingWriteLock())
        {
            _handlers.Add(handler);
        }
    }

    public void Unregister(Action<Memory<byte>> handler)
    {
        using (_lock.UsingWriteLock())
        {
            _handlers.Remove(handler);
        }
    }
}

public static partial class EventServiceLoggerExtension
{
    [LoggerMessage(EventId = 0, Level = LogLevel.Debug, Message = "BotOfflineEvent {{ {reason} {tag} {message} }}")]
    public static partial void LogOffline(this ILogger<EventService> logger, Reasons reason, string? tag, string? message);

    [LoggerMessage(EventId = 1, Level = LogLevel.Debug, Message = "BotMessageEvent {{ {type} {group} {sender} {entities} }}")]
    public static partial void LogGroupMessage(this ILogger<EventService> logger, MessageType type, long group, long sender, string entities);

    [LoggerMessage(EventId = 2, Level = LogLevel.Debug, Message = "BotMessageEvent {{ {type} {sender} {entities} }}")]
    public static partial void LogPrivateMessage(this ILogger<EventService> logger, MessageType type, long sender, string entities);

    [LoggerMessage(EventId = 3, Level = LogLevel.Debug, Message = "BotGroupNudgeEvent {{ group: {group}, sender: {sender} target: {target} }}")]
    public static partial void LogGroupNudgeEvent(this ILogger<EventService> logger, long group, long sender, long target);

    [LoggerMessage(EventId = 4, Level = LogLevel.Debug, Message = "BotGroupMemberDecreaseEvent {{ group: {group}, user: {user}, operator: {operator} }}")]
    public static partial void LogGroupMemberDecreaseEvent(this ILogger<EventService> logger, long group, long user, long? @operator);

    [LoggerMessage(EventId = 5, Level = LogLevel.Debug, Message = "BotFriendRequestEvent {{ request: {request}, user: {user}, message: {message}, source: {source} }}")]
    public static partial void LogBotFriendRequestEvent(this ILogger<EventService> logger, string request, long user, string? message, string? source);

    [LoggerMessage(EventId = 6, Level = LogLevel.Debug, Message = "BotGroupInviteEvent {{ request: {request}, user: {user}, group: {group} }}")]
    public static partial void LogGroupInvitationEvent(this ILogger<EventService> logger, long request, long user, long group);

    [LoggerMessage(EventId = 7, Level = LogLevel.Debug, Message = "BotGroupMemberIncreaseEvent {{ group: {group}, user: {user}, operator: {operator}, invitor: {invitor} }}")]
    public static partial void LogGroupMemberIncreaseEvent(this ILogger<EventService> logger, long group, long user, long? @operator, long? invitor);

    [LoggerMessage(EventId = 999, Level = LogLevel.Error, Message = "Handle {event} exception")]
    public static partial void LogHandleEventException(this ILogger<EventService> logger, string @event, Exception e);
}