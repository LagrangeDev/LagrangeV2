using System.Net;
using System.Net.Http.Headers;
using Lagrange.Milky.Implementation.Common.Api.Results;
using Lagrange.Milky.Implementation.Configuration;
using Lagrange.Milky.Implementation.Extension;
using Lagrange.Milky.Utility;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Lagrange.Milky.Implementation.Api;

public class MilkyApiHandler(ILogger<MilkyApiHandler> logger, IOptionsSnapshot<MilkyConfiguration> config, IServiceProvider services)
{
    private readonly MilkyConfiguration _config = config.Value;

    public async Task Handle(HttpListenerContext http, string api, CancellationToken token)
    {
        var request = http.Request;
        var response = http.Response;
        var identifier = http.Request.RequestTraceIdentifier;

        if (!MediaTypeHeaderValue.TryParse(request.ContentType, out var mediaType) || mediaType.MediaType != "application/json")
        {
            logger.LogSend(identifier, HttpStatusCode.UnsupportedMediaType);
            response.SendUnsupportedMediaType();
            return;
        }

        var handler = services.GetKeyedService<IApiHandler>(api);
        if (handler == null)
        {
            logger.LogSend(identifier, HttpStatusCode.NotFound);
            http.Response.SendNotFound();
            return;
        }

        using var reader = new StreamReader(request.InputStream);
        string body = await reader.ReadToEndAsync(token);
        logger.LogReceived(identifier, body);

        object param;
        try
        {
            param = JsonHelper.Deserialize(handler.ParamType, body) ?? throw new NullReferenceException();
        }
        catch (Exception e)
        {
            logger.LogDeserializeParamFailed(identifier, e);
            http.Response.SendBadRequest();
            throw;
        }

        IApiResult result;
        try
        {
            result = await handler.HandleAsync(param, token);
        }
        catch (Exception e)
        {
            logger.LogHandleApiFailed(identifier, e);
            result = new ApiFailedResult
            {
                Retcode = long.MinValue,
                Message = "HandleApiFailed"
            };
        }

        byte[] resultBytes = JsonHelper.SerializeToUtf8Bytes(result.GetType(), result);
        http.Response.ContentType = "application/json; charset=utf-8";
        await http.Response.OutputStream.WriteAsync(resultBytes, token);
        http.Response.Close();
    }

    public bool ValidateApiAccessToken(HttpListenerContext http)
    {
        var authorization = http.Request.Headers["Authorization"];

        if (_config.AccessToken == null) return true;
        if (authorization == null) return false;
        if (!authorization.StartsWith("Bearer ")) return false;

        return authorization["Bearer ".Length..] == _config.AccessToken;
    }
}

public static partial class LoggerHelper
{
    [LoggerMessage(EventId = 1, Level = LogLevel.Information, Message = "{identifier} << {status}")]
    public static partial void LogSend(this ILogger<MilkyApiHandler> logger, Guid identifier, HttpStatusCode status);

    [LoggerMessage(EventId = 2, Level = LogLevel.Information, Message = "{identifier} >> {payload}")]
    public static partial void LogReceived(this ILogger<MilkyApiHandler> logger, Guid identifier, string? payload);


    [LoggerMessage(EventId = 998, Level = LogLevel.Information, Message = "{identifier} >< handle api failed")]
    public static partial void LogHandleApiFailed(this ILogger<MilkyApiHandler> logger, Guid identifier, Exception e);

    [LoggerMessage(EventId = 999, Level = LogLevel.Information, Message = "{identifier} >< Deserialize param failed")]
    public static partial void LogDeserializeParamFailed(this ILogger<MilkyApiHandler> logger, Guid identifier, Exception e);
}