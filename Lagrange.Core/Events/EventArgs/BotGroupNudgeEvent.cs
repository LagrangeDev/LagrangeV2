namespace Lagrange.Core.Events.EventArgs;

public class BotGroupNudgeEvent(long groupUin, long operatorUin, string action, string actionImgUrl, long targetUin, string suffix) : EventBase
{
    public long GroupUin { get; } = groupUin;

    public long OperatorUin { get; } = operatorUin;

    public string Action { get; } = action;

    public string ActionImageUrl { get; } = actionImgUrl;

    public long TargetUin { get; } = targetUin;

    public string Suffix { get; } = suffix;

    public override string ToEventMessage()
    {
        return $"{nameof(BotGroupNudgeEvent)}: In Group {GroupUin}, {OperatorUin} {Action}({ActionImageUrl}) {TargetUin} {Suffix}";
    }
}
