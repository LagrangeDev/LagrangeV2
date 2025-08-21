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
        new GroupNudgeEventData(@event.GroupUin, @event.OperatorUin, @event.TargetUin)
    );
    
    public GroupMemberDecreaseEvent GroupMemberDecreaseEvent(LgrEventArgs.BotGroupMemberDecreaseEvent @event) => new(
        @event.EventTime.ToUnixTimeSeconds(),
        _bot.BotUin,
        new GroupMemberDecreaseEventData(@event.GroupUin, @event.UserUin, @event.OperatorUin == 0 ? null : @event.OperatorUin)
    );
    
    public FriendRequestEvent FriendRequestEvent(LgrEventArgs.BotFriendRequestEvent @event) => new(
        @event.EventTime.ToUnixTimeSeconds(),
        _bot.BotUin,
        new FriendRequestEventData(@event.InitiatorUid, @event.InitiatorUin, 
            @event.Message, @event.Source)
    );
    
    public GroupInvitationEvent GroupInvitationEvent(LgrEventArgs.BotGroupInviteSelfEvent @event) => new(
        @event.EventTime.ToUnixTimeSeconds(),
        _bot.BotUin,
        new GroupInvitationEventData(@event.InvitationSeq,
            @event.InitiatorUin, @event.GroupUin)
    );
}
