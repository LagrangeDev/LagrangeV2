using System.Text.Json.Serialization;

namespace Lagrange.Milky.Entity.Segment;

[method: JsonConstructor]
public class ForwardIncomingSegment(ForwardIncomingSegmentData data) : IncomingSegmentBase<ForwardIncomingSegmentData>(data)
{
    public ForwardIncomingSegment(string forwardId) : this(new ForwardIncomingSegmentData(forwardId)) { }
}

public class ForwardIncomingSegmentData(string forwardId)
{
    [JsonPropertyName("forward_id")]
    public string ForwardId { get; } = forwardId;
}

public class ForwardOutgoingSegment(ForwardOutgoingSegmentData data) : OutgoingSegmentBase<ForwardOutgoingSegmentData>(data) { }

public class ForwardOutgoingSegmentData(ForwardOutgoingSegmentDataItem[] messages)
{
    [JsonRequired]
    [JsonPropertyName("messages")]
    public ForwardOutgoingSegmentDataItem[] Messages { get; init; } = messages;
}

public class ForwardOutgoingSegmentDataItem(long userId, string senderName, IOutgoingSegment[] segments)
{
    [JsonPropertyName("user_id")]
    public long UserId { get; init; } = userId;

    [JsonPropertyName("sender_name")]
    public string SenderName { get; init; } = senderName;

    [JsonPropertyName("segments")]
    public IOutgoingSegment[] Segments { get; init; } = segments;
}