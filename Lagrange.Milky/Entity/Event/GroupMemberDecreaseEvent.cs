using System.Text.Json.Serialization;

namespace Lagrange.Milky.Entity.Event;

public class GroupMemberDecreaseEvent(long time, long selfId, GroupMemberDecreaseEventData data) : EventBase<GroupMemberDecreaseEventData>(time, selfId, "group_member_decrease", data) { }

public class GroupMemberDecreaseEventData
{
    [JsonPropertyName("group_id")]
    public long GroupID { get; }
    [JsonPropertyName("user_id")]
    public long UserID { get; }
    [JsonPropertyName("operator_id")]
    public long? OperatorID { get; }

    public GroupMemberDecreaseEventData(long groupId, long userId, long? operatorId)
    {
        GroupID = groupId;
        UserID = userId;
        OperatorID = operatorId;
    }
}