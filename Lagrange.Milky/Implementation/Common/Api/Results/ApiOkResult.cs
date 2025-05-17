using System.Text.Json.Serialization;

namespace Lagrange.Milky.Implementation.Common.Api.Results;

public class ApiOkResult<TData> : IApiResult
{
    public string Status => "ok";
    public long Retcode => 0;
    public required TData Data { get; init; }
}