using System.Net;
using Lagrange.Milky.Implementation.Api;
using Lagrange.Milky.Implementation.Configuration;
using Lagrange.Milky.Implementation.Events;
using Lagrange.Milky.Implementation.Extension;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Lagrange.Milky.Implementation.Services;

public class MilkyService(ILogger<MilkyService> logger, IOptionsSnapshot<MilkyConfiguration> config, MilkyApiHandler api, MilkyEventHandler @event) : IHostedService
{
    private readonly string _prefix = $"http://{config.Value.Host}:{config.Value.Port}{config.Value.CommonPrefix}";
    private readonly string _apiPath = $"{config.Value.CommonPrefix}api/";
    private readonly string _eventPath = $"{config.Value.CommonPrefix}event";

    private Task? _task;
    private CancellationTokenSource? _cts;

    public Task StartAsync(CancellationToken token)
    {
        var listener = new HttpListener();
        listener.Prefixes.Add(_prefix);
        listener.Start();

        foreach (string prefix in listener.Prefixes) logger.LogServerStarted(prefix);

        _cts = CancellationTokenSource.CreateLinkedTokenSource(token);
        _task = ListenerLoopAsync(listener, _cts.Token);
        if (_task.IsCompleted)
        {
            return _task;
        }

        return Task.CompletedTask;
    }

    protected async Task ListenerLoopAsync(HttpListener listener, CancellationToken token)
    {
        try
        {
            while (true)
            {
                _ = HandleHttpListenerContext(await listener.GetContextAsync().WaitAsync(token), token);
            }
        }
        catch (Exception e) when (e is not OperationCanceledException)
        {
            logger.LogGetHttpContextException(e);
        }
    }

    private async Task HandleHttpListenerContext(HttpListenerContext http, CancellationToken token)
    {
        var request = http.Request;
        var response = http.Response;

        var identifier = request.RequestTraceIdentifier;

        try
        {
            logger.LogConnect(identifier, request.RemoteEndPoint, request.HttpMethod, request.Url?.LocalPath);

            HttpMethod method = HttpMethod.Parse(request.HttpMethod);
            string? path = request.Url?.LocalPath;
            if (method == HttpMethod.Post && (path?.StartsWith(_apiPath) ?? false))
            {
                if (!api.ValidateApiAccessToken(http))
                {
                    logger.LogAccessTokenValidationFailed(identifier);

                    response.SendForbidden();

                    return;
                }

                await api.Handle(http, path[_apiPath.Length..], token);
            }
            else if (path == _eventPath) await @event.Handle(http, token);
            else response.SendNotFound();
        }
        catch (Exception e)
        {
            logger.LogHandleHttpListenerContextException(identifier, e);
            response.SendInternalServerError();
        }
    }

    public async Task StopAsync(CancellationToken token)
    {
        await (_cts?.CancelAsync() ?? Task.CompletedTask);
        if (_task != null) await _task.WaitAsync(token).ConfigureAwait(ConfigureAwaitOptions.SuppressThrowing);
    }
}

public static partial class LoggerHelper
{
    [LoggerMessage(EventId = 0, Level = LogLevel.Information, Message = "The server is started at {prefix}")]
    public static partial void LogServerStarted(this ILogger<MilkyService> logger, string prefix);

    [LoggerMessage(EventId = 1, Level = LogLevel.Information, Message = "{identifier} >> {remote} {method} {path}")]
    public static partial void LogConnect(this ILogger<MilkyService> logger, Guid identifier, IPEndPoint remote, string method, string? path);

    [LoggerMessage(EventId = 997, Level = LogLevel.Warning, Message = "{identifier} >< Access token validation failed")]
    public static partial void LogAccessTokenValidationFailed(this ILogger<MilkyService> logger, Guid identifier);

    [LoggerMessage(EventId = 998, Level = LogLevel.Critical, Message = "{identifier} >< Handle HttpListenerContext filed")]
    public static partial void LogHandleHttpListenerContextException(this ILogger<MilkyService> logger, Guid identifier, Exception e);

    [LoggerMessage(EventId = 999, Level = LogLevel.Critical, Message = "Get http context failed")]
    public static partial void LogGetHttpContextException(this ILogger<MilkyService> logger, Exception e);
}