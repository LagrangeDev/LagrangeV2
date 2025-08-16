using System.Text.Json.Serialization;

namespace Lagrange.Milky.Entity.Segment;

[method: JsonConstructor]
public class ImageIncomingSegment(ImageIncomingSegmentData data) : IncomingSegmentBase<ImageIncomingSegmentData>(data)
{
    public ImageIncomingSegment(string resourceId, string tempUrl, int width, int height, string summary, string subType) : this(new ImageIncomingSegmentData(resourceId, tempUrl, width, height, summary, subType)) { }
}

public class ImageOutgoingSegment(ImageOutgoingSegmentData data) : OutgoingSegmentBase<ImageOutgoingSegmentData>(data) { }

public class ImageIncomingSegmentData(string resourceId, string tempUrl, int width, int height, string summary, string subType)
{
    [JsonPropertyName("resource_id")]
    public string ResourceId { get; } = resourceId;

    [JsonPropertyName("temp_url")]
    public string TempUrl { get; } = tempUrl;

    [JsonPropertyName("width")]
    public int Width { get; } = width;

    [JsonPropertyName("height")]
    public int Height { get; } = height;

    [JsonPropertyName("summary")]
    public string Summary { get; } = summary;

    [JsonPropertyName("sub_type")]
    public string SubType { get; } = subType;
}

public class ImageOutgoingSegmentData(string uri, string summary, string subType)
{
    [JsonRequired]
    [JsonPropertyName("uri")]
    public string Uri { get; init; } = uri;

    [JsonPropertyName("summary")]
    public string Summary { get; init; } = summary;

    [JsonRequired]
    [JsonPropertyName("sub_type")]
    public string SubType { get; init; } = subType;
}