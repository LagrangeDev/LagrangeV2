using System.Net.Http.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using ProtoBuf;

namespace Lagrange.Core.Common;

public interface IBotSignProvider
{
    protected static readonly string[] WhiteListCommand =
    [
        "trpc.o3.ecdh_access.EcdhAccess.SsoEstablishShareKey",
        "trpc.o3.ecdh_access.EcdhAccess.SsoSecureAccess",
        "trpc.o3.report.Report.SsoReport",
        "MessageSvc.PbSendMsg",
        "wtlogin.trans_emp",
        "wtlogin.login",
        "wtlogin.exchange_emp",
        "trpc.login.ecdh.EcdhService.SsoKeyExchange",
        "trpc.login.ecdh.EcdhService.SsoNTLoginPasswordLogin",
        "trpc.login.ecdh.EcdhService.SsoNTLoginEasyLogin",
        "trpc.login.ecdh.EcdhService.SsoNTLoginPasswordLoginNewDevice",
        "trpc.login.ecdh.EcdhService.SsoNTLoginEasyLoginUnusualDevice",
        "trpc.login.ecdh.EcdhService.SsoNTLoginPasswordLoginUnusualDevice",
        "OidbSvcTrpcTcp.0x11ec_1",
        "OidbSvcTrpcTcp.0x758_1", // create group
        "OidbSvcTrpcTcp.0x7c1_1",
        "OidbSvcTrpcTcp.0x7c2_5", // request friend
        "OidbSvcTrpcTcp.0x10db_1",
        "OidbSvcTrpcTcp.0x8a1_7", // request group
        "OidbSvcTrpcTcp.0x89a_0",
        "OidbSvcTrpcTcp.0x89a_15",
        "OidbSvcTrpcTcp.0x88d_0", // fetch group detail
        "OidbSvcTrpcTcp.0x88d_14",
        "OidbSvcTrpcTcp.0x112a_1",
        "OidbSvcTrpcTcp.0x587_74",
        "OidbSvcTrpcTcp.0x1100_1",
        "OidbSvcTrpcTcp.0x1102_1",
        "OidbSvcTrpcTcp.0x1103_1",
        "OidbSvcTrpcTcp.0x1107_1",
        "OidbSvcTrpcTcp.0x1105_1",
        "OidbSvcTrpcTcp.0xf88_1",
        "OidbSvcTrpcTcp.0xf89_1",
        "OidbSvcTrpcTcp.0xf57_1",
        "OidbSvcTrpcTcp.0xf57_106",
        "OidbSvcTrpcTcp.0xf57_9",
        "OidbSvcTrpcTcp.0xf55_1",
        "OidbSvcTrpcTcp.0xf67_1",
        "OidbSvcTrpcTcp.0xf67_5",
        "OidbSvcTrpcTcp.0x6d9_4"
    ];
    
    internal static bool IsWhiteListCommand(string cmd) => WhiteListCommand.Contains(cmd);

    Task<SsoSecureInfo?> GetSecSign(long uin, string cmd, int seq, ReadOnlyMemory<byte> body);
}

internal class DefaultBotSignProvider(Protocols protocol, BotAppInfo appInfo) : IBotSignProvider, IDisposable
{
    private readonly HttpClient _client = new();
    
    private readonly string _url = protocol switch
    {
        Protocols.Windows => throw new NotSupportedException("Windows is not supported"),
        Protocols.MacOs => throw new NotSupportedException("MacOs is not supported"),
        Protocols.Linux => $"https://sign.lagrangecore.org/api/sign/{appInfo.AppClientVersion}",
        Protocols.AndroidPhone => throw new NotSupportedException("AndroidPhone is not supported"),
        Protocols.AndroidPad => throw new NotSupportedException("AndroidPad is not supported"),
        _ => throw new ArgumentOutOfRangeException(nameof(protocol))
    };
    
    public async Task<SsoSecureInfo?> GetSecSign(long uin, string cmd, int seq, ReadOnlyMemory<byte> body)
    {
        try
        {
            var payload = new JsonObject
            {
                ["cmd"] = cmd,
                ["seq"] = seq,
                ["src"] = Convert.ToHexString(body.Span),
            };
            
            var response = await _client.PostAsJsonAsync(_url, payload);
            if (!response.IsSuccessStatusCode) return null;
            
            var content = await response.Content.ReadFromJsonAsync<Root>();
            if (content == null) return null;
            
            return new SsoSecureInfo
            {
                SecSign = Convert.FromHexString(content.Value.Sign),
                SecToken = Convert.FromHexString(content.Value.Token), 
                SecExtra = Convert.FromHexString(content.Value.Extra)
            };
        }
        catch (Exception e)
        {
            // TODO: Log the exception
            return null;
        }
    }
    
    public void Dispose()
    {
        _client.Dispose();
    }

    [Serializable]
    private class Root
    {
        [JsonPropertyName("value")] public Response Value { get; set; } = new();
    }
    
    [Serializable]
    private class Response
    {
        [JsonPropertyName("sign")] public string Sign { get; set; } = string.Empty;
        
        [JsonPropertyName("token")] public string Token { get; set; } = string.Empty;
        
        [JsonPropertyName("extra")] public string Extra { get; set; } = string.Empty;
    }
}

[Serializable]
[ProtoContract]
public class SsoSecureInfo
{
    [ProtoMember(1)] public byte[]? SecSign { get; set; }
    
    [ProtoMember(2)] public byte[]? SecToken { get; set; }
    
    [ProtoMember(3)] public byte[]? SecExtra { get; set; }
}