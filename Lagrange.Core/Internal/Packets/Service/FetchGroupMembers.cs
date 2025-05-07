using Lagrange.Proto;

namespace Lagrange.Core.Internal.Packets.Service;

#pragma warning disable CS8618

[ProtoPackable]
internal partial class FetchGroupMembersRequest
{
    [ProtoMember(1)] public uint GroupUin { get; set; }

    [ProtoMember(2)] public uint Field2 { get; set; }

    [ProtoMember(3)] public uint Field3 { get; set; }

    [ProtoMember(4)] public FetchGroupMembersRequestBody Body { get; set; }

    [ProtoMember(15)] public byte[]? Cookie { get; set; }
}

[ProtoPackable]
internal partial class FetchGroupMembersRequestBody
{
    [ProtoMember(10)] public bool MemberName { get; set; } = true;

    [ProtoMember(11)] public bool MemberCard { get; set; } = true;

    [ProtoMember(12)] public bool Level { get; set; } = true;

    [ProtoMember(13)] public bool Field13 { get; set; } = true;

    [ProtoMember(16)] public bool Field16 { get; set; } = true;

    [ProtoMember(17)] public bool SpecialTitle { get; set; } = true;

    [ProtoMember(18)] public bool Field18 { get; set; } = true;

    [ProtoMember(20)] public bool Field20 { get; set; } = true;

    [ProtoMember(21)] public bool Field21 { get; set; } = true;

    [ProtoMember(100)] public bool JoinTimestamp { get; set; } = true;

    [ProtoMember(101)] public bool LastMsgTimestamp { get; set; } = true;

    [ProtoMember(102)] public bool ShutUpTimestamp { get; set; } = true;

    [ProtoMember(103)] public bool Field103 { get; set; } = true;

    [ProtoMember(104)] public bool Field104 { get; set; } = true;

    [ProtoMember(105)] public bool Field105 { get; set; } = true;

    [ProtoMember(106)] public bool Field106 { get; set; } = true;

    [ProtoMember(107)] public bool Permission { get; set; } = true;

    [ProtoMember(200)] public bool Field200 { get; set; } = true;

    [ProtoMember(201)] public bool Field201 { get; set; } = true;
}

[ProtoPackable]
internal partial class FetchGroupMembersResponse
{
    [ProtoMember(1)] public long GroupUin { get; set; }

    [ProtoMember(2)] public List<FetchGroupMembersResponseMember> Members { get; set; }

    [ProtoMember(3)] public uint MemberCount { get; set; }

    [ProtoMember(5)] public uint MemberListChangeSeq { get; set; }

    [ProtoMember(6)] public uint MemberCardSeq { get; set; }

    [ProtoMember(15)] public byte[]? Cookie { get; set; }
}

[ProtoPackable]
internal partial class FetchGroupMembersResponseMember
{
    [ProtoMember(1)] public FetchGroupMembersResponseId Id { get; set; }

    [ProtoMember(10)] public string MemberName { get; set; }

    [ProtoMember(17)] public string? SpecialTitle { get; set; }

    [ProtoMember(11)] public FetchGroupMembersResponseCard MemberCard { get; set; }

    [ProtoMember(12)] public FetchGroupMembersResponseLevel? Level { get; set; }

    [ProtoMember(100)] public uint JoinTimestamp { get; set; }

    [ProtoMember(101)] public uint LastMsgTimestamp { get; set; }

    [ProtoMember(102)] public uint ShutUpTimestamp { get; set; }

    [ProtoMember(107)] public uint Permission { get; set; }
}

[ProtoPackable]
internal partial class FetchGroupMembersResponseId
{
    [ProtoMember(2)] public string Uid { get; set; }

    [ProtoMember(4)] public uint Uin { get; set; }
}

[ProtoPackable]
internal partial class FetchGroupMembersResponseCard
{
    [ProtoMember(2)] public string? MemberCard { get; set; }
}

[ProtoPackable]
internal partial class FetchGroupMembersResponseLevel
{
    [ProtoMember(1)] public List<uint>? Infos { get; set; }

    [ProtoMember(2)] public uint Level { get; set; }
}