using System.Text.Json.Serialization;

namespace Lagrange.Milky.Entity.Event;


public class GroupInvitationEvent(long time, long selfId, GroupInvitationEventData data) : EventBase<GroupInvitationEventData>(time, selfId, "group_invitation", data) { }

public class GroupInvitationEventData(long invitationSeq, long initiatorId, long groupId)
{
    [JsonPropertyName("invitation_seq")] 
    public long InvitationSeq { get; } = invitationSeq;

    [JsonPropertyName("initiator_id")] 
    public long InitiatorID { get; } = initiatorId;

    [JsonPropertyName("group_id")] 
    public long GroupID { get; } = groupId;
}