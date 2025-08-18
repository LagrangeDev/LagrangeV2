namespace Lagrange.Core.Events.EventArgs;

public class BotGroupInvitationEvent(long invitationSeq, long initiatorUin, long groupUin) : EventBase
{
    public long InvitationSeq { get; } = invitationSeq;

    public long InitiatorUin { get; } = initiatorUin;
    
    public long GroupUin { get; } = groupUin;

    public override string ToEventMessage()
    {
        return $"{nameof(BotGroupInvitationEvent)}: InvitationSeq: {InvitationSeq}, InitiatorUin: {InitiatorUin}, GroupUin: {GroupUin}";
    }
}