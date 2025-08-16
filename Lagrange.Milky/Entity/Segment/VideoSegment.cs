using System.Text.Json.Serialization;

namespace Lagrange.Milky.Entity.Segment;

[method: JsonConstructor]
public class VideoIncomingSegment(VideoIncomingSegmentData data) : IncomingSegmentBase<VideoIncomingSegmentData>(data)
{
    public VideoIncomingSegment(string resourceId, string tempUrl, int width, int height, int duration) : this(new VideoIncomingSegmentData(resourceId, tempUrl, width, height, duration)) { }
}

public class VideoOutgoingSegment(VideoOutgoingSegmentData data) : OutgoingSegmentBase<VideoOutgoingSegmentData>(data) { }

public class VideoIncomingSegmentData(string resourceId, string tempUrl, int width, int height, int duration)
{
    [JsonPropertyName("resource_id")]
    public string ResourceId { get; } = resourceId;

    [JsonPropertyName("temp_url")]
    public string TempUrl { get; } = tempUrl;

    [JsonPropertyName("width")]
    public int Width { get; } = width;

    [JsonPropertyName("height")]
    public int Height { get; } = height;

    [JsonPropertyName("duration")]
    public int Duration { get; } = duration;
}

public class VideoOutgoingSegmentData(string uri, string? thumbUri)
{
    [JsonRequired]
    [JsonPropertyName("uri")]
    public string Uri { get; init; } = uri;

    [JsonPropertyName("thumb_uri")]
    public string? ThumbUri { get; } = thumbUri;
}