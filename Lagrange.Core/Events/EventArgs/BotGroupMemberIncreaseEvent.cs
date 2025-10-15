namespace Lagrange.Core.Events.EventArgs;

public class BotGroupMemberIncreaseEvent(long groupUin, long memberUin, long invitorUin, uint type, long? operatorUin) : EventBase
{
    public long GroupUin { get; } = groupUin;
    public long MemberUin { get; } = memberUin;
    public long InvitorUin { get; } = invitorUin;
    public uint Type { get; } = type;
    public long? OperatorUin { get; } = operatorUin;


    public override string ToEventMessage()
    {
        return $"{nameof(BotGroupMemberIncreaseEvent)}: GroupUin={GroupUin}, MemberUin={MemberUin}, InvitorUin={InvitorUin}, Type={Type}, OperatorUin={OperatorUin}";
    }
}