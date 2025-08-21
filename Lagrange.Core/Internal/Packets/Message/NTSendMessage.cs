using Lagrange.Proto;

namespace Lagrange.Core.Internal.Packets.Message;

#pragma warning disable CS8618

[ProtoPackable]
internal partial class PbSendMsgReq
{
    [ProtoMember(1)] public SendRoutingHead RoutingHead { get; set; }
    
    [ProtoMember(2)] public SendContentHead ContentHead { get; set; }
    
    [ProtoMember(3)] public MessageBody MessageBody { get; set; }
    
    [ProtoMember(4)] public ulong ClientSequence { get; set; }
    
    [ProtoMember(5)] public uint Random { get; set; }
}

[ProtoPackable]
internal partial class PbSendMsgResp
{
    [ProtoMember(1)] public int Result { get; set; }
	
    [ProtoMember(2)] public string? ErrMsg { get; set; }
	
    [ProtoMember(3)] public long SendTime { get; set; }
    
    [ProtoMember(10)] public uint MsgInfoFlag { get; set; }
    
    [ProtoMember(11)] public ulong Sequence { get; set; }
    
    [ProtoMember(14)] public ulong ClientSequence { get; set; }
}

[ProtoPackable]
internal partial class SendContentHead
{
    [ProtoMember(1)] public uint PkgNum { get; set; } // 分包数目，消息需要分包发送时该值不为1
    [ProtoMember(2)] public uint PkgIndex { get; set; } // 当前分包索引，从 0开始
    
    [ProtoMember(3)] public uint DivSeq { get; set; } // 消息分包的序列号，同一条消息的各个分包的 div_seq 相同
    
    [ProtoMember(4)] public uint AutoReply { get; set; }
}

[ProtoPackable]
internal partial class SendRoutingHead // EncodeSendMsgReqRoutingHead
{
    [ProtoMember(1)] public C2C C2C { get; set; }
    
    [ProtoMember(2)] public Grp Group { get; set; }
    
    [ProtoMember(15)] public Trans0X211 Trans0X211 { get; set; }
}

[ProtoPackable]
internal partial class C2C
{
    [ProtoMember(1)] public long PeerUin { get; set; }
    
    [ProtoMember(2)] public string PeerUid { get; set; }
}

[ProtoPackable]
internal partial class Grp
{
    [ProtoMember(1)] public long GroupUin { get; set; } // group_uin
}

[ProtoPackable]
internal partial class Trans0X211
{
    [ProtoMember(1)] public long ToUin { get; set; }
    
    [ProtoMember(2)] public uint CcCmd { get; set; }
    
    [ProtoMember(8)] public string Uid { get; set; }
}