using System.Text.Json.Serialization;

namespace Lagrange.Milky.Implementation.Common.Api.Results;

public class ApiFailedResult : IApiResult
{
    public string Status => "failed";
    public required long Retcode { get; init; }
    public required string Message { get; init; }
}