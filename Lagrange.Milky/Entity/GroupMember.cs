using System.Text.Json.Serialization;

namespace Lagrange.Milky.Entity;

public class GroupMember(long userId, string nickname, string sex, long groupId, string card, string title, int level, string role, long joinTime, long lastSentTime, long? shutUpEndTime)
{
    [JsonPropertyName("user_id")]
    public long UserId { get; } = userId;

    [JsonPropertyName("nickname")]
    public string Nickname { get; } = nickname;

    [JsonPropertyName("sex")]
    public string Sex { get; } = sex;

    [JsonPropertyName("group_id")]
    public long GroupId { get; } = groupId;

    [JsonPropertyName("card")]
    public string Card { get; } = card;

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("title")]
    public string Title { get; } = title;

    [JsonPropertyName("level")]
    public int Level { get; } = level;

    [JsonPropertyName("role")]
    public string Role { get; } = role;

    [JsonPropertyName("join_time")]
    public long JoinTime { get; } = joinTime;

    [JsonPropertyName("last_sent_time")]
    public long LastSentTime { get; } = lastSentTime;

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("shut_up_end_time")]
    public long? ShutUpEndTime { get; } = shutUpEndTime;
}