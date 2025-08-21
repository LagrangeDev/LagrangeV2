namespace Lagrange.Core.Events.EventArgs;

public class BotGroupReactionEvent(long targetGroupUin, ulong targetSequence, long operatorUin, bool isAdd, string code, ulong currentCount) : EventBase
{
    public long TargetGroupUin { get; } = targetGroupUin;

    public ulong TargetSequence { get; } = targetSequence;

    public long OperatorUin { get; } = operatorUin;

    public bool IsAdd { get; } = isAdd;

    public string Code { get; } = code;

    public ulong CurrentCount { get; } = currentCount;

    public override string ToEventMessage()
    {
        return $"{nameof(BotGroupReactionEvent)}: Target {TargetGroupUin} > {TargetSequence} Operator {OperatorUin} {(IsAdd ? "Add" : "Reduce")} {Code} Current {CurrentCount}";
    }
}
