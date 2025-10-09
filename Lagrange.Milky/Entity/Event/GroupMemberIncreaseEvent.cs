using System.Text.Json.Serialization;

namespace Lagrange.Milky.Entity.Event;

public class GroupMemberIncreaseEvent(long time, long selfId, GroupMemberIncreaseEventData data) : EventBase<GroupMemberIncreaseEventData>(time, selfId, "group_member_increase", data) { }

public class GroupMemberIncreaseEventData(long groupId, long userId, long? operatorId, long? invitorId = null)
{
    [JsonPropertyName("group_id")]
    public long GroupId { get; } = groupId;
    [JsonPropertyName("user_id")]
    public long UserId { get; } = userId;
    [JsonPropertyName("operator_id")]
    public long? OperatorId { get; } = operatorId;
    [JsonPropertyName("invitor_id")]
    public long? InvitorId { get; } = invitorId;
}