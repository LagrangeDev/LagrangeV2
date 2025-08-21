namespace Lagrange.Core.Events.EventArgs;

public class BotGroupInviteSelfEvent(long invitationSeq, long initiatorUin, long groupUin) : EventBase
{
    public long InvitationSeq { get; } = invitationSeq;

    public long InitiatorUin { get; } = initiatorUin;
    
    public long GroupUin { get; } = groupUin;

    public override string ToEventMessage()
    {
        return $"{nameof(BotGroupInviteSelfEvent)}: InvitationSeq: {InvitationSeq}, InitiatorUin: {InitiatorUin}, GroupUin: {GroupUin}";
    }
}