using System.Text.Json.Serialization;

namespace Lagrange.Milky.Entity;

public class Friend(long userId, string qid, string nickname, string sex, string remark, FriendCategory category)
{
    [JsonPropertyName("user_id")]
    public long UserId { get; } = userId;

    [JsonPropertyName("nickname")]
    public string Nickname { get; } = nickname;

    [JsonPropertyName("sex")]
    public string Sex { get; } = sex;

    [JsonPropertyName("qid")]
    public string Qid { get; } = qid;

    [JsonPropertyName("remark")]
    public string Remark { get; } = remark;

    [JsonPropertyName("category")]
    public FriendCategory Category { get; } = category;
}