using System.Text.Json.Serialization;

namespace Lagrange.Milky.Implementation.Common.Api.Params;

public class CachedParam
{
    [JsonPropertyName("no_cache")]
    public bool? NoCache { get; init; }
}