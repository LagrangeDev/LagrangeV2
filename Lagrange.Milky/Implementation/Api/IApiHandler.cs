using Lagrange.Milky.Implementation.Common.Api.Params;
using Lagrange.Milky.Implementation.Common.Api.Results;

namespace Lagrange.Milky.Implementation.Api;

public interface IApiHandler
{
    Type ParamType { get; }

    ValueTask<IApiResult> HandleAsync(object param, CancellationToken token);
}

public interface IApiHandler<in T> : IApiHandler
{
    Type IApiHandler.ParamType => typeof(T);

    ValueTask<IApiResult> IApiHandler.HandleAsync(object param, CancellationToken token) => HandleAsync((T)param, token);

    ValueTask<IApiResult> HandleAsync(T param, CancellationToken token);
}

public interface IEmptyParamApiHandler : IApiHandler<EmptyParam>
{
    ValueTask<IApiResult> IApiHandler<EmptyParam>.HandleAsync(EmptyParam param, CancellationToken token) => HandleAsync(token);

    ValueTask<IApiResult> HandleAsync(CancellationToken token);
}