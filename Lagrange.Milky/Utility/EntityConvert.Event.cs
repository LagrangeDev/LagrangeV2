using Lagrange.Milky.Entity.Event;
using Lagrange.Milky.Extension;
using LgrEventArgs = Lagrange.Core.Events.EventArgs;

namespace Lagrange.Milky.Utility;

public partial class EntityConvert
{
    public BotOfflineEvent BotOfflineEvent(LgrEventArgs.BotOfflineEvent @event) => new(
        @event.EventTime.ToUnixTimeSeconds(),
        _bot.BotUin,
        new BotOfflineEventData($"{@event.Reason} {@event.Tips?.Tag} {@event.Tips?.Message}")
    );

    public MessageReceiveEvent MessageReceiveEvent(LgrEventArgs.BotMessageEvent @event) => new(
        @event.Message.Time.ToUnixTimeSeconds(),
        _bot.BotUin,
        MessageBase(@event.Message)
    );

    public GroupNudgeEvent GroupNudgeEvent(LgrEventArgs.BotGroupNudgeEvent @event) => new(
        @event.EventTime.ToUnixTimeSeconds(),
        _bot.BotUin,
        new GroupNudgeEventData(@event.GroupUin, @event.OperatorUin, @event.TargetUin, @event.Action, @event.Suffix, @event.ActionImageUrl)
    );

    public GroupMemberIncreaseEvent GroupMemberIncreaseEvent(LgrEventArgs.BotGroupMemberIncreaseEvent @event) => new(
        @event.EventTime.ToUnixTimeSeconds(),
        _bot.BotUin,
        new GroupMemberIncreaseEventData(
            @event.GroupUin,
            @event.MemberUin,
            @event.OperatorUin,
            @event.InvitorUin
        )
    );
    
    public GroupMemberDecreaseEvent GroupMemberDecreaseEvent(LgrEventArgs.BotGroupMemberDecreaseEvent @event) => new(
        @event.EventTime.ToUnixTimeSeconds(),
        _bot.BotUin,
        new GroupMemberDecreaseEventData(
            @event.GroupUin,
            @event.UserUin,
            @event.OperatorUin == 0 ? null : @event.OperatorUin
        )
    );

    public FriendRequestEvent FriendRequestEvent(LgrEventArgs.BotFriendRequestEvent @event) => new(
        @event.EventTime.ToUnixTimeSeconds(),
        _bot.BotUin,
        new FriendRequestEventData(
            @event.InitiatorUid,
            @event.InitiatorUin,
            @event.Message,
            @event.Source
        )
    );
}
