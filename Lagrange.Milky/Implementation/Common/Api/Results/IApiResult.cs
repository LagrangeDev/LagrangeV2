namespace Lagrange.Milky.Implementation.Common.Api.Results;

public interface IApiResult
{
    public string Status { get; }

    public long Retcode { get; }
}