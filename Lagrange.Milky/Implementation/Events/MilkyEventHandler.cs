using System.Net;
using Lagrange.Milky.Implementation.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Lagrange.Milky.Implementation.Events;

public class MilkyEventHandler(ILogger<MilkyEventHandler> logger, IOptionsSnapshot<MilkyConfiguration> options)
{
    private readonly ILogger<MilkyEventHandler> _logger = logger;
    private readonly MilkyConfiguration _configuration = options.Value;

    public Task Handle(HttpListenerContext http, CancellationToken token)
    {
        // TODO implement event handler
        http.Response.Close();
        throw new NotImplementedException();
    }

    private bool ValidateApiAccessToken(HttpListenerContext http)
    {
        if (_configuration.AccessToken == null) return true;

        return http.Request.QueryString["access_token"] == _configuration.AccessToken;
    }
}