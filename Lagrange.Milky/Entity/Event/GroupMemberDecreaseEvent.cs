using System.Text.Json.Serialization;

namespace Lagrange.Milky.Entity.Event;

public class GroupMemberDecreaseEvent(long time, long selfId, GroupMemberDecreaseEventData data) : EventBase<GroupMemberDecreaseEventData>(time, selfId, "group_member_decrease", data) { }

public class GroupMemberDecreaseEventData(long groupId, long userId, long? operatorId)
{
    [JsonPropertyName("group_id")]
    public long GroupId { get; } = groupId;
    [JsonPropertyName("user_id")]
    public long UserId { get; } = userId;
    [JsonPropertyName("operator_id")]
    public long? OperatorId { get; } = operatorId;
}