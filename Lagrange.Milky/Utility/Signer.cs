using System.Net;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Text.Json.Serialization;
using Lagrange.Core.Common;
using Lagrange.Milky.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Lagrange.Milky.Utility;

public sealed class Signer : BotSignProvider, IDisposable
{
    private static readonly HashSet<string> PcWhiteListCommand =
    [
        "trpc.o3.ecdh_access.EcdhAccess.SsoEstablishShareKey", "trpc.o3.ecdh_access.EcdhAccess.SsoSecureAccess",
        "trpc.o3.report.Report.SsoReport", "MessageSvc.PbSendMsg", "wtlogin.trans_emp", "wtlogin.login",
        "wtlogin.exchange_emp", "trpc.login.ecdh.EcdhService.SsoKeyExchange",
        "trpc.login.ecdh.EcdhService.SsoNTLoginPasswordLogin", "trpc.login.ecdh.EcdhService.SsoNTLoginEasyLogin",
        "trpc.login.ecdh.EcdhService.SsoNTLoginPasswordLoginNewDevice",
        "trpc.login.ecdh.EcdhService.SsoNTLoginEasyLoginUnusualDevice",
        "trpc.login.ecdh.EcdhService.SsoNTLoginPasswordLoginUnusualDevice",
        "trpc.login.ecdh.EcdhService.SsoNTLoginRefreshTicket", "trpc.login.ecdh.EcdhService.SsoNTLoginRefreshA2",
        "OidbSvcTrpcTcp.0x11ec_1", "OidbSvcTrpcTcp.0x758_1", "OidbSvcTrpcTcp.0x7c1_1", "OidbSvcTrpcTcp.0x7c2_5",
        "OidbSvcTrpcTcp.0x10db_1", "OidbSvcTrpcTcp.0x8a1_7", "OidbSvcTrpcTcp.0x89a_0", "OidbSvcTrpcTcp.0x89a_15",
        "OidbSvcTrpcTcp.0x88d_0", "OidbSvcTrpcTcp.0x88d_14", "OidbSvcTrpcTcp.0x112a_1", "OidbSvcTrpcTcp.0x587_74",
        "OidbSvcTrpcTcp.0x1100_1", "OidbSvcTrpcTcp.0x1102_1", "OidbSvcTrpcTcp.0x1103_1", "OidbSvcTrpcTcp.0x1107_1",
        "OidbSvcTrpcTcp.0x1105_1", "OidbSvcTrpcTcp.0xf88_1", "OidbSvcTrpcTcp.0xf89_1", "OidbSvcTrpcTcp.0xf57_1",
        "OidbSvcTrpcTcp.0xf57_106", "OidbSvcTrpcTcp.0xf57_9", "OidbSvcTrpcTcp.0xf55_1", "OidbSvcTrpcTcp.0xf67_1",
        "OidbSvcTrpcTcp.0xf67_5", "OidbSvcTrpcTcp.0x6d9_4"
    ];

    private readonly ILogger<Signer> _logger;

    private readonly string _url;
    private readonly HttpClient _client;

    private readonly long _uin;
    private readonly string? _token;

    public Signer(ILogger<Signer> logger, IOptions<CoreConfiguration> options)
    {
        _logger = logger;

        var signerConfiguration = options.Value.Signer;
        _url = signerConfiguration.Url ?? throw new Exception("Core.Signer.Url cannot be null");
        _client = new HttpClient(new HttpClientHandler
        {
            Proxy = signerConfiguration.ProxyUrl == null ? null : new WebProxy
            {
                Address = new Uri(signerConfiguration.ProxyUrl),
                BypassProxyOnLocal = false,
                UseDefaultCredentials = false,
            },
        });

        _uin = options.Value.Login.Uin ?? 0;
        _token = signerConfiguration.Token;
    }

    public override bool IsWhiteListCommand(string cmd) => PcWhiteListCommand.Contains(cmd);

    public override async Task<SsoSecureInfo?> GetSecSign(long uin, string cmd, int seq, ReadOnlyMemory<byte> body)
    {
        try
        {
            using var request = new HttpRequestMessage();
            request.Method = HttpMethod.Post;
            request.RequestUri = new Uri($"{_url}{(_url.EndsWith('/') ? "" : "/")}api/sign/sec-sign");
            if (!string.IsNullOrEmpty(_token))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _token);
            }
            request.Content = new StringContent(
                JsonUtility.Serialize(new SecSignRequest
                {
                    Uin = uin == 0 ? _uin : uin,
                    Command = cmd,
                    Sequence = seq,
                    Body = Convert.ToHexString(body.Span).ToLower(),
                    Guid = Convert.ToHexString(Context.Keystore.Guid).ToLower(),
                    Qua = Context.AppInfo.Qua,
                }),
                System.Text.Encoding.UTF8,
                MediaTypeNames.Application.Json
            );

            using var response = await _client.SendAsync(request);
            response.EnsureSuccessStatusCode();

            using var stream = await response.Content.ReadAsStreamAsync();
            var result = JsonUtility.Deserialize<SignerResponse<SecSignResponse>>(stream);
            if (result == null) throw new Exception("Signer response serialization failed");
            if (result.Code != 0) throw new Exception($"Signer server exception: ({result.Code}){result.Message}");

            return new SsoSecureInfo
            {
                SecSign = Convert.FromHexString(result.Value.SecSign),
                SecToken = Convert.FromHexString(result.Value.SecToken),
                SecExtra = Convert.FromHexString(result.Value.SecExtra),
            };
        }
        catch (Exception e)
        {
            _logger.LogGetSecSignFailed(e);
            return null;
        }
    }


    public void Dispose()
    {
        _client.Dispose();
    }
}

public static partial class SignerLoggerExtension
{
    [LoggerMessage(LogLevel.Error, "Get sec sign failed")]
    public static partial void LogGetSecSignFailed(this ILogger<Signer> logger, Exception e);
}

public class SignerResponse<T>
{
    [JsonPropertyName("code")]
    public required int Code { get; init; }

    [JsonPropertyName("message")]
    public required string? Message { get; init; }

    [JsonPropertyName("value")]
    public required T Value { get; init; }
}

public class SecSignRequest
{
    [JsonPropertyName("uin")]
    public required long Uin { get; init; }

    [JsonPropertyName("command")]
    public required string Command { get; init; }

    [JsonPropertyName("seq")]
    public required int Sequence { get; init; }

    [JsonPropertyName("body")]
    public required string Body { get; init; }

    [JsonPropertyName("guid")]
    public required string Guid { get; init; }

    [JsonPropertyName("qua")]
    public required string Qua { get; init; }
}

public class SecSignResponse
{
    [JsonPropertyName("sec_sign")]
    public required string SecSign { get; init; }

    [JsonPropertyName("sec_token")]
    public required string SecToken { get; init; }

    [JsonPropertyName("sec_extra")]
    public required string SecExtra { get; init; }
}