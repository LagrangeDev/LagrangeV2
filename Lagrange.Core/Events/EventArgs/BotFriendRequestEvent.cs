namespace Lagrange.Core.Events.EventArgs;

public class BotFriendRequestEvent(string initiatorUid, long initiatorUin, string message, string source) : EventBase
{
    public string InitiatorUid { get; } = initiatorUid;

    public long InitiatorUin { get; } = initiatorUin;
    
    public string Message { get; } = message;
    
    public string Source { get; } = source;

    public override string ToEventMessage()
    {
        return $"{nameof(BotFriendRequestEvent)}: InitiatorUid: {InitiatorUid}, InitiatorUin: {InitiatorUin}, Message: {Message}, Source: {Source}";
    }
}
