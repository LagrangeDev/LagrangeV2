namespace Lagrange.Core.Events.EventArgs;

public class BotGroupNudgeEvent(long groupUin, long operatorUin, long targetUin) : EventBase
{
    public long GroupUin { get; } = groupUin;

    public long OperatorUin { get; } = operatorUin;

    public long TargetUin { get; } = targetUin;

    public override string ToEventMessage()
    {
        return $"{nameof(BotGroupNudgeEvent)}: GroupUin={GroupUin}, OperatorUin={OperatorUin}, TargetUin={TargetUin}";
    }
}
