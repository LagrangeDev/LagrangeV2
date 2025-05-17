namespace Lagrange.Milky.Implementation.Api.Exceptions;

public class ApiHandlerException(long retcode, string message) : Exception(message)
{
    public long Retcode { get; } = retcode;
}