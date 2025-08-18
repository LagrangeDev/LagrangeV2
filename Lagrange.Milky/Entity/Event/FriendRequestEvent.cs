using System.Text.Json.Serialization;
using Lagrange.Milky.Extension;

namespace Lagrange.Milky.Entity.Event;

public class FriendRequestEvent(long time, long selfId, FriendRequestEventData data) : EventBase<FriendRequestEventData>(time, selfId, "friend_request", data) { }

public class FriendRequestEventData(string initiatorUid, long initiatorId, string comment, string via)
{
    [JsonPropertyName("initiator_uid")]
    public string InitiatorUID { get; } = initiatorUid;
    [JsonPropertyName("initiator_id")]
    public long InitiatorID { get; } = initiatorId;
    [JsonPropertyName("comment")]
    public string Comment { get; } = comment;
    [JsonPropertyName("via")]
    public string Via { get; } = via;
}