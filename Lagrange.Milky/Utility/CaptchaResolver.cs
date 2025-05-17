using System.Net;
using System.Text.Json.Nodes;
using Lagrange.Core;
using Microsoft.Extensions.Logging;

namespace Lagrange.Milky.Utility;

public interface ICaptchaResolver
{
    Task<(string, string)> ResolveCaptchaAsync(string url, CancellationToken token = default);
}

public class ManualCaptchaResolver : ICaptchaResolver
{
    public Task<(string, string)> ResolveCaptchaAsync(string url, CancellationToken token)
    {
        Console.WriteLine($"Captcha URL: {url}");
        Console.Write("Please enter the ticket: ");
        string ticket = Console.ReadLine() ?? string.Empty;
        Console.Write("Please enter the randstr: ");
        string randstr = Console.ReadLine() ?? string.Empty;

        return Task.FromResult((ticket, randstr));
    }
}

public partial class OnlineCaptchaResolver(ILogger<OnlineCaptchaResolver> logger, BotContext bot) : ICaptchaResolver
{
    private readonly HttpClient _client = new();

    private const string Url = "https://captcha.lagrangecore.org/?{0}";

    private const string QueryUrl = "https://backend.captcha.lagrangecore.org/get_captcha?uin={0}";

    public async Task<(string, string)> ResolveCaptchaAsync(string url, CancellationToken token)
    {
        string solveUrl = string.Format(Url, url.Split('?')[1].Replace("uin=0", $"uin={bot.BotUin}"));
        QrCodeHelper.Output(solveUrl, false);
        Log.Captcha(logger, solveUrl);

        string ticket, randstr;
        while (true)
        {
            token.ThrowIfCancellationRequested();

            try
            {
                string queryUrl = string.Format(QueryUrl, bot.BotUin);
                var response = await _client.GetAsync(queryUrl, token);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    string result = await response.Content.ReadAsStringAsync(token);
                    string? json = JsonNode.Parse(result)?["data"]?.GetValue<string>();
                    if (json == null) continue;

                    ticket = json.Split('|')[0];
                    randstr = json.Split('|')[1];
                    break;
                }

                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    Log.CaptchaWaiting(logger);
                }
                else
                {
                    Log.CaptchaError(logger, response.StatusCode.ToString());
                }
            }
            catch (Exception e)
            {
                Log.CaptchaError(logger, e.Message);
            }
        }

        return (ticket, randstr);
    }

    private static partial class Log
    {
        [LoggerMessage(Level = LogLevel.Information, Message = "Please scan the QR code or access the URL to solve the captcha\n{url}")]
        public static partial void Captcha(ILogger logger, string url);

        [LoggerMessage(Level = LogLevel.Information, Message = "Captcha solved, ticket: {ticket}, randstr: {randstr}")]
        public static partial void CaptchaSolved(ILogger logger, string ticket, string randstr);

        [LoggerMessage(Level = LogLevel.Debug, Message = "Waiting for captcha response...")]
        public static partial void CaptchaWaiting(ILogger logger);

        [LoggerMessage(Level = LogLevel.Warning, Message = "Captcha error: {error}")]
        public static partial void CaptchaError(ILogger logger, string error);
    }
}