namespace Lagrange.Core.Events.EventArgs;

public class BotGroupMemberDecreaseEvent(Int64 groupUin, Int64 userUin, Int64 operatorUin) : EventBase
{
    public Int64 GroupUin { get; } = groupUin;
    
    public Int64 UserUin { get; } = userUin;
    
    public Int64 OperatorUin { get; } = operatorUin;

    public override string ToEventMessage()
    {
        return $"{nameof(BotGroupNudgeEvent)}: GroupUin={GroupUin}, OperatorUin={OperatorUin}, TargetUin={UserUin}";
    }
}