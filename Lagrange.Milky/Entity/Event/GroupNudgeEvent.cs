using System.Text.Json.Serialization;

namespace Lagrange.Milky.Entity.Event;

public class GroupNudgeEvent(long time, long selfId, GroupNudgeEventData data) : EventBase<GroupNudgeEventData>(time, selfId, "group_nudge", data) { }

public class GroupNudgeEventData(long groupID, long senderId, long receiverId, string displayAction, string displaySuffix, string displayActionImgUrl)
{
    [JsonPropertyName("group_id")]
    public long GroupID { get; } = groupID;

    [JsonPropertyName("sender_id")]
    public long SenderID { get; } = senderId;

    [JsonPropertyName("receiver_id")]
    public long ReceiverID { get; } = receiverId;

    [JsonPropertyName("display_action")]
    public string DisplayAction = displayAction;

    [JsonPropertyName("display_suffix")]
    public string DisplaySuffix { get; } = displaySuffix;

    [JsonPropertyName("display_action_img_url")]
    public string DisplayActionImgUrl { get; } = displayActionImgUrl;
}