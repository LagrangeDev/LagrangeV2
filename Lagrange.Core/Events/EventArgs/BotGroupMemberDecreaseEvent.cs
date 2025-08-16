namespace Lagrange.Core.Events.EventArgs;

public class BotGroupMemberDecreaseEvent(long groupUin, long userUin, long? operatorUin) : EventBase
{
    public long GroupUin { get; } = groupUin;

    public long UserUin { get; } = userUin;

    public long? OperatorUin { get; } = operatorUin;

    public override string ToEventMessage()
    {
        return $"{nameof(BotGroupMemberDecreaseEvent)}: GroupUin={GroupUin}, UserUin={UserUin}, OperatorUin={OperatorUin}";
    }
}