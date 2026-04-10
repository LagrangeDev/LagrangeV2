using System.Text.Json.Serialization;

namespace Lagrange.Milky.Entity.Segment;

[method: JsonConstructor]
public class LightAppIncomingSegment(LightAppIncomingSegmentData data) : IncomingSegmentBase<LightAppIncomingSegmentData>(data)
{
    public LightAppIncomingSegment(string appName, string jsonPayload) : this(new LightAppIncomingSegmentData(appName, jsonPayload)) { }
}

[method: JsonConstructor]
public class LightAppOutgoingSegment(LightAppOutgoingSegmentData data) : OutgoingSegmentBase<LightAppOutgoingSegmentData>(data)
{
    public LightAppOutgoingSegment(string jsonPayload) : this(new LightAppOutgoingSegmentData(jsonPayload)) { }
}

public class LightAppIncomingSegmentData(string appName, string jsonPayload)
{
    [JsonPropertyName("app_name")]
    public string AppName { get; } = appName;

    [JsonPropertyName("json_payload")]
    public string JsonPayload { get; } = jsonPayload;
}

public class LightAppOutgoingSegmentData(string jsonPayload)
{

    [JsonPropertyName("json_payload")]
    public string JsonPayload { get; } = jsonPayload;
}