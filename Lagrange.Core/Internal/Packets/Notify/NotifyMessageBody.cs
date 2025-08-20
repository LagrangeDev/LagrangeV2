using Lagrange.Proto;

namespace Lagrange.Core.Internal.Packets.Notify;

#pragma warning disable CS8618

[ProtoPackable]
internal partial class NotifyMessageBody
{
    [ProtoMember(1)] public uint NotifyType { get; set; }

    [ProtoMember(4)] public long GroupUin { get; set; }

    [ProtoMember(5)] public byte[]? EventParam { get; set; }

    [ProtoMember(11)] public GroupRecall Recall { get; set; }

    [ProtoMember(13)] public uint SubType { get; set; }

    [ProtoMember(21)] public string OperatorUid { get; set; }

    [ProtoMember(26)] public GeneralGrayTipInfo GeneralGrayTip { get; set; }

    [ProtoMember(33)] public EssenceMessage EssenceMessage;

    [ProtoMember(37)] public ulong MsgSequence { get; set; }

    [ProtoMember(39)] public uint Field39 { get; set; }

    [ProtoMember(40)] public GroupRecallNudge GroupRecallNudge { get; set; }

    [ProtoMember(44)] public GroupReactionData0 Reaction { get; set; }

    [ProtoMember(50)] public ulong TipsSeqId { get; set; }
}

[ProtoPackable]
internal partial class GeneralGrayTipInfo
{
    [ProtoMember(1)] public ulong BusiType { get; set; }

    [ProtoMember(2)] public ulong BusiId { get; set; }

    [ProtoMember(3)] public uint CtrlFlag { get; set; }

    [ProtoMember(4)] public uint C2CType { get; set; }

    [ProtoMember(5)] public uint ServiceType { get; set; }

    [ProtoMember(6)] public ulong TemplId { get; set; }

    [ProtoMember(7)] public List<TemplParam> MsgTemplParam { get; set; }

    [ProtoMember(8)] public string Content { get; set; }

    [ProtoMember(10)] public ulong TipsSeqId { get; set; }

    [ProtoMember(100)] public GrayTipMsgInfo MsgInfo { get; set; }
}

[ProtoPackable]
internal partial class GrayTipMsgInfo
{
    [ProtoMember(6)] public ulong Sequence { get; set; }
}

[ProtoPackable]
internal partial class TemplParam
{
    [ProtoMember(1)] public string Name { get; set; }

    [ProtoMember(2)] public string Value { get; set; }
}

[ProtoPackable]
internal partial class FriendRecallPokeInfo
{
    [ProtoMember(1)] public string SelfUid { get; set; }

    [ProtoMember(2)] public string PeerUid { get; set; }

    [ProtoMember(3)] public string OperatorUid { get; set; }

    [ProtoMember(4)] public ulong BusiId { get; set; }

    [ProtoMember(5)] public ulong TipsSeqId { get; set; }
}
