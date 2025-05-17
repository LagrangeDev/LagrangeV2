using System.Text.Json.Serialization;

namespace Lagrange.Milky.Implementation.Common.Api.Results;

public class ApiOkResult<TData> : IApiResult
{
    [JsonPropertyName("status")]
    public string Status => "ok";

    [JsonPropertyName("retcode")]
    public long Retcode => 0;

    [JsonPropertyName("data")]
    public required TData Data { get; init; }
}