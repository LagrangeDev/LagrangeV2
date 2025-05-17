using Lagrange.Proto;

namespace Lagrange.Core.Internal.Packets.Login;

#pragma warning disable CS8618 // Non-nullable field is uninitialized.

/// <summary>
/// Request for trpc.login.ecdh.EcdhService.SsoKeyExchange
/// </summary>
[ProtoPackable]
internal partial class KeyExchangeRequest
{
    [ProtoMember(1)] public byte[] PublicKey { get; set; } = [];
    
    [ProtoMember(2)] public uint Type { get; set; }
        
    [ProtoMember(3)] public byte[] Secret { get; set; } = []; // buf_secret
    
    [ProtoMember(4)] public long Timestamp { get; set; }
    
    [ProtoMember(5)] public byte[] VerifyHash { get; set; } = [];
}

[ProtoPackable]
internal partial class KeyExchangeResponse
{
    [ProtoMember(1)] public byte[] Secret { get; set; }
    
    [ProtoMember(2)] public byte[] Field2 { get; set; }
    
    [ProtoMember(3)] public byte[] PublicKey { get; set; }
}

[ProtoPackable]
internal partial class KeyExchangeRequestBuf // pb_buf
{
    [ProtoMember(1)] public string? Uin { get; set; }
    
    [ProtoMember(2)] public byte[]? Guid { get; set; }
}

[ProtoPackable]
internal partial class KeyExchangeResponseSecret
{
    [ProtoMember(1)] public byte[] SessionKey { get; set; }
    
    [ProtoMember(2)] public byte[] SessionTicket { get; set; }
    
    [ProtoMember(3)] public uint Expiration { get; set; }
}

[ProtoPackable]
internal partial class NTLoginForwardRequest
{
    [ProtoMember(1)] public byte[] SessionTicket { get; set; }
    
    [ProtoMember(2)] public byte[] Buffer { get; set; }
    
    [ProtoMember(3)] public uint Type { get; set; }
}