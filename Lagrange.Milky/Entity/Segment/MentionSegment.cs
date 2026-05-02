using System.Text.Json.Serialization;

namespace Lagrange.Milky.Entity.Segment;

[method: JsonConstructor]
public class MentionIncomingSegment(MentionIncomingSegmentData data) : IncomingSegmentBase<MentionIncomingSegmentData>(data)
{
    public MentionIncomingSegment(long userId, string name) : this(new MentionIncomingSegmentData(userId, name)) { }
}

public class MentionOutgoingSegment(MentionOutgoingSegmentData data) : OutgoingSegmentBase<MentionOutgoingSegmentData>(data) { }

public class MentionIncomingSegmentData(long userId, string name)
{
    [JsonPropertyName("user_id")]
    public long UserId { get; } = userId;

    [JsonPropertyName("name")]
    public string Name { get; } = name;
}

public class MentionOutgoingSegmentData(long userId)
{
    [JsonPropertyName("user_id")]
    public long UserId { get; } = userId;
}