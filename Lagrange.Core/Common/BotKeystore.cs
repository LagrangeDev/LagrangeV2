using System.Security.Cryptography;
using System.Text.Json.Serialization;
using Lagrange.Core.Utility.Cryptography;

namespace Lagrange.Core.Common;

public class BotKeystore
{
    [JsonConstructor]
    public BotKeystore() { }
    
    public long Uin { get; set; }
    
    public string Uid { get; set; } = string.Empty;

    internal EcdhProvider Prime256V1 { get; } = new(EllipticCurve.Prime256V1);
    internal EcdhProvider Secp192K1 { get; } = new(EllipticCurve.Secp192K1);
    internal State State { get; } = new();

    public BotInfo? BotInfo { get; set; }
    public WLoginSigs WLoginSigs { get; set; } = new();
    
    public byte[] Guid { get; set; } = [];
    public string AndroidId { get; set; } = string.Empty;
    public string Qimei { get; set; } = string.Empty;
    public string DeviceName { get; set; } = string.Empty;

    public static BotKeystore CreateEmpty()
    {
        var guid = new byte[16];
        Random.Shared.NextBytes(guid);
        
        var androidId = new byte[8];
        Random.Shared.NextBytes(androidId);
        
        return new BotKeystore
        {
            Guid = guid,
            AndroidId = Convert.ToHexString(androidId),
            DeviceName = $"Lagrange-{Random.Shared.Next(0, 16777215):X6}",
        };
    }
}

public class WLoginSigs
{
    public byte[] A2 { get; set; } = []; // Tlv10A
    
    public byte[] A2Key { get; set; } = new byte[16]; // Tlv10D
    
    public byte[] D2 { get; set; } = []; // Tlv143
    
    public byte[] D2Key { get; set; } = new byte[16]; // Tlv305
    
    public byte[] A1 { get; set; } = []; // Tlv106
    
    public byte[] A1Key { get; set; } = new byte[16]; // Tlv10C

    public byte[] NoPicSig { get; set; } = [];// Tlv16A
    
    public byte[] TgtgtKey { get; set; } = [];
    
    public byte[] Ksid { get; set; } = [];
    
    public byte[] SuperKey { get; set; } = [];
    
    public byte[] StKey { get; set; } = [];
    
    public byte[] StWeb { get; set; } = [];
    
    public byte[] St { get; set; } = [];
    
    public byte[] WtSessionTicket { get; set; } = [];
    
    public byte[] WtSessionTicketKey { get; set; } = [];
    
    public byte[] RandomKey { get; set; } = new byte[16];
    
    public byte[] SKey { get; set; } = [];
    
    public Dictionary<string, string> PsKey { get; set; } = new();

    public void Clear()
    {
        A2 = [];
        D2 = [];
        D2Key = new byte[16];
        A1 = [];
        TgtgtKey = [];
        RandomKey = new byte[16];
        RandomNumberGenerator.Fill(RandomKey);
    }
}

internal class State
{
    public byte[]? Tlv104 { get; set; }
    
    public byte[]? Tlv547 { get; set; }
    
    public byte[]? Tlv174 { get; set; }
    
    public (byte[] SessionTicket, byte[] SessionKey)? KeyExchangeSession { get; set; }
    
    public string? Cookie { get; set; }
    
    public byte[]? QrSig { get; set; }
}