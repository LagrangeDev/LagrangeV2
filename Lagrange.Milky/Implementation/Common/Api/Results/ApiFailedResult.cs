using System.Text.Json.Serialization;

namespace Lagrange.Milky.Implementation.Common.Api.Results;

public class ApiFailedResult : IApiResult
{
    [JsonPropertyName("status")]
    public string Status => "failed";

    [JsonPropertyName("retcode")]
    public required long Retcode { get; init; }

    [JsonPropertyName("message")]
    public required string Message { get; init; }
}