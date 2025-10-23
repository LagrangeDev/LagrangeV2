using Lagrange.Proto;

namespace Lagrange.Core.Internal.Packets.Message;

#pragma warning disable CS8618

[ProtoPackable]
internal partial class CommonMessage
{
    [ProtoMember(1)] public RoutingHead RoutingHead { get; set; }
    
    [ProtoMember(2)] public ContentHead ContentHead { get; set; }
    
    [ProtoMember(3)] public MessageBody? MessageBody { get; set; }
}

[ProtoPackable]
internal partial class ContentHead
{
    [ProtoMember(1)] public int Type { get; set; }
    
    [ProtoMember(2)] public int SubType { get; set; }
    
    [ProtoMember(3)] public int C2CCommand { get; set; } // c2c_cmd
    
    [ProtoMember(4)] public uint Random { get; set; }
    
    [ProtoMember(5)] public ulong Sequence { get; set; } // msg_seq
    
    [ProtoMember(6)] public long Time { get; set; }
    
    [ProtoMember(7)] public int PkgNum { get; set; }
    
    [ProtoMember(8)] public int PkgIndex { get; set; }
    
    [ProtoMember(9)] public int DivSeq { get; set; }
    
    [ProtoMember(10)] public int AutoReply { get; set; }
    
    [ProtoMember(11)] public ulong ClientSequence { get; set; } // nt_msg_seq
    
    [ProtoMember(12)] public ulong MsgUid { get; set; }
}

[ProtoPackable]
internal partial class RoutingHead
{
    [ProtoMember(1)] public long FromUin { get; set; }

    [ProtoMember(2)] public string FromUid { get; set; }

    [ProtoMember(3)] public int FromAppId { get; set; }

    [ProtoMember(4)] public int FromInstId { get; set; }

    [ProtoMember(5)] public long ToUin { get; set; }

    [ProtoMember(6)] public string ToUid { get; set; }
    
    [ProtoMember(7)] public CommonC2C CommonC2C { get; set; }
    
    [ProtoMember(8)] public CommonGroup Group { get; set; }
}

[ProtoPackable]
internal partial class CommonC2C
{
    [ProtoMember(1)] public int C2CType { get; set; } // 2 for group, which is the only type that NTQQ Supports
    
    [ProtoMember(2)] public int ServiceType { get; set; }

    [ProtoMember(3)] public byte[]? Sig { get; set; }
    
    [ProtoMember(4)] public long FromTinyId { get; set; }
    
    [ProtoMember(5)] public long ToTinyId { get; set; }

    [ProtoMember(6)] public string Name { get; set; }
}

[ProtoPackable]
internal partial class CommonGroup
{
    [ProtoMember(1)] public long GroupCode { get; set; }

    [ProtoMember(2)] public int GroupType { get; set; }

    [ProtoMember(3)] public long GroupInfoSeq { get; set; }

    [ProtoMember(4)] public string GroupCard { get; set; } = string.Empty;

    [ProtoMember(5)] public int GroupCardType { get; set; }

    [ProtoMember(6)] public int GroupLevel { get; set; }

    [ProtoMember(7)] public string GroupName { get; set; } = string.Empty;

    [ProtoMember(8)] public string ExtGroupKeyInfo { get; set; } = string.Empty;

    [ProtoMember(9)] public int MsgFlag { get; set; }
}

[ProtoPackable]
internal partial class MessageBody
{
    [ProtoMember(1)] public RichText RichText { get; set; } = new();

    [ProtoMember(2)] public ReadOnlyMemory<byte>? MsgContent { get; set; }

    [ProtoMember(3)] public byte[]? MsgEncryptContent { get; set; }
}

[ProtoPackable]
internal partial class RichText
{
    [ProtoMember(1)] public Attr? Attr { get; set; }

    [ProtoMember(2)] public List<Elem> Elems { get; set; } = [];
    
    [ProtoMember(3)] public NotOnlineFile? NotOnlineFile { get; set; }

    [ProtoMember(4)] public Ptt? Ptt { get; set; }

    [ProtoMember(5)] public TmpPtt? TmpPtt { get; set; }
    
    [ProtoMember(6)] public Trans211TmpMsg? Trans211TmpMsg { get; set; }
}

[ProtoPackable]
internal partial class Attr
{
    [ProtoMember(1)] public int CodePage { get; set; }

    [ProtoMember(2)] public int Time { get; set; }

    [ProtoMember(3)] public int PlayModeRandom { get; set; }

    [ProtoMember(4)] public int Color { get; set; }

    [ProtoMember(5)] public int Size { get; set; }

    [ProtoMember(6)] public int TabEffect { get; set; }

    [ProtoMember(7)] public int CharSet { get; set; }

    [ProtoMember(8)] public int PitchAndFamily { get; set; }

    [ProtoMember(9)] public string FontName { get; set; } = "Times New Roman";

    [ProtoMember(10)] public byte[]? ReserveData { get; set; }
}

[ProtoPackable]
internal partial class NotOnlineFile
{
    [ProtoMember(1)] public uint FileType { get; set; }
    
    [ProtoMember(2)] public byte[]? Sig { get; set; }
    
    [ProtoMember(3)] public string FileUuid { get; set; }
    
    [ProtoMember(4)] public byte[]? FileMd5 { get; set; }
    
    [ProtoMember(5)] public string FileName { get; set; }
    
    [ProtoMember(6)] public ulong FileSize { get; set; }
    
    [ProtoMember(7)] public byte[]? Note { get; set; }
    
    [ProtoMember(8)] public uint Reserved { get; set; }
    
    [ProtoMember(9)] public uint SubCmd { get; set; }
    
    [ProtoMember(10)] public uint MicroCloud { get; set; }

    [ProtoMember(11)] public List<byte[]> FileUrls { get; set; } = [];
    
    [ProtoMember(12)] public uint DownloadFlag { get; set; }
    
    [ProtoMember(50)] public uint DangerLevel { get; set; }
    
    [ProtoMember(51)] public uint LifeTime { get; set; }
    
    [ProtoMember(52)] public uint UploadTime { get; set; }
    
    [ProtoMember(53)] public uint AbsFileType { get; set; }
    
    [ProtoMember(54)] public uint ClientType { get; set; }
    
    [ProtoMember(55)] public uint ExpireTime { get; set; }
    
    [ProtoMember(56)] public byte[]? PbReserve { get; set; }
    
    [ProtoMember(57)] public string FileIdCrcMedia { get; set; } = string.Empty;
}

[ProtoPackable]
internal partial class Ptt
{
    [ProtoMember(1)] public uint FileType { get; set; }

    [ProtoMember(2)] public ulong SrcUin { get; set; }

    [ProtoMember(3)] public byte[]? FileUuid { get; set; }

    [ProtoMember(4)] public byte[]? FileMd5 { get; set; }

    [ProtoMember(5)] public string FileName { get; set; }

    [ProtoMember(6)] public uint FileSize { get; set; }

    [ProtoMember(7)] public byte[]? Reserve { get; set; }

    [ProtoMember(8)] public uint FileId { get; set; }

    [ProtoMember(9)] public uint ServerIp { get; set; }

    [ProtoMember(10)] public uint ServerPort { get; set; }

    [ProtoMember(11)] public bool Valid { get; set; }

    [ProtoMember(12)] public byte[]? Signature { get; set; }

    [ProtoMember(13)] public byte[]? Shortcut { get; set; }

    [ProtoMember(14)] public string? FileKey { get; set; }

    [ProtoMember(15)] public uint MagicPttIndex { get; set; }

    [ProtoMember(16)] public uint VoiceSwitch { get; set; }

    [ProtoMember(17)] public byte[]? PttUrl { get; set; }

    [ProtoMember(18)] public byte[]? GroupFileKey { get; set; }

    [ProtoMember(19)] public uint Time { get; set; }

    [ProtoMember(20)] public byte[]? DownPara { get; set; }

    [ProtoMember(29)] public uint Format { get; set; }

    [ProtoMember(30)] public byte[]? PbReserve { get; set; }

    [ProtoMember(31)] public List<byte[]> PttUrls { get; set; } = [];

    [ProtoMember(32)] public uint DownloadFlag { get; set; }
}

[ProtoPackable]
internal partial class TmpPtt
{
    [ProtoMember(1)] public uint FileType { get; set; }

    [ProtoMember(2)] public byte[]? FileUuid { get; set; }

    [ProtoMember(3)] public byte[]? FileMd5 { get; set; }

    [ProtoMember(4)] public byte[]? FileName { get; set; }

    [ProtoMember(5)] public uint FileSize { get; set; }

    [ProtoMember(6)] public uint PttTimes { get; set; }

    [ProtoMember(7)] public uint UserType { get; set; }

    [ProtoMember(8)] public uint PttTransFlag { get; set; }

    [ProtoMember(9)] public uint BusiType { get; set; }

    [ProtoMember(10)] public ulong MsgId { get; set; }

    [ProtoMember(30)] public byte[]? PbReserve { get; set; }

    [ProtoMember(31)] public byte[]? PttEncodeData { get; set; }
}

[ProtoPackable]
internal partial class Trans211TmpMsg
{
    [ProtoMember(1)] public byte[]? MsgBody { get; set; }

    [ProtoMember(2)] public uint C2CCmd { get; set; }
}