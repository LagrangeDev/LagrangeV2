using Lagrange.Proto;

namespace Lagrange.Core.Internal.Packets.Notify;

#pragma warning disable CS8618

[ProtoPackable]
internal partial class EssenceMessage
{
    [ProtoMember(1)] public long GroupUin;

    [ProtoMember(2)] public ulong MsgSequence;

    [ProtoMember(3)] public uint Random;

    [ProtoMember(4)] public uint SetFlag; // set 1  remove 2

    [ProtoMember(5)] public uint MemberUin;

    [ProtoMember(6)] public uint OperatorUin;

    [ProtoMember(7)] public uint TimeStamp;

    [ProtoMember(8)] public ulong MsgSequence2; // removed 0

    [ProtoMember(9)] public string OperatorNickName;

    [ProtoMember(10)] public string MemberNickName;

    [ProtoMember(11)] public uint SetFlag2;
}
