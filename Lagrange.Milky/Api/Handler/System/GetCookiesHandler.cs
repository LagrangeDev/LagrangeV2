using System.Text.Json.Serialization;
using Lagrange.Core;
using Lagrange.Core.Common.Interface;
using Lagrange.Milky.Api.Exception;

namespace Lagrange.Milky.Api.Handler.System;

[Api("get_cookies")]
public class GetCookiesHandler(BotContext bot) : IApiHandler<GetCookiesParameter, GetCookiesResult>
{
    private readonly BotContext _bot = bot;

    public async Task<GetCookiesResult> HandleAsync(GetCookiesParameter parameter, CancellationToken token)
    {
        var cookies = await _bot.FetchCookies(parameter.Domain);
        if (!cookies.TryGetValue(parameter.Domain, out string? cookie))
        {
            throw new ApiException(-1, "cookie not found");
        }
        return new GetCookiesResult(cookie);
    }
}

public class GetCookiesParameter(string domain)
{
    [JsonRequired]
    [JsonPropertyName("domain")]
    public string Domain { get; init; } = domain;
}

public class GetCookiesResult(string cookies)
{
    [JsonPropertyName("cookies")]
    public string Cookies { get; } = cookies;
}