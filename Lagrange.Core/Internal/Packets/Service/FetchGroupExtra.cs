using Lagrange.Proto;

namespace Lagrange.Core.Internal.Packets.Service;

#pragma warning disable CS8618

[ProtoPackable]
internal partial class FetchGroupExtraRequest
{
    [ProtoMember(1)]
    public long Random { get; set; }

    [ProtoMember(2)]
    public FetchGroupExtraRequestConfig Config { get; set; }
}

[ProtoPackable]
internal partial class FetchGroupExtraRequestConfig
{
    [ProtoMember(1)]
    public long GroupUin { get; set; }

    [ProtoMember(2)]
    public FetchGroupExtraRequestConfigFlags Flags { get; set; }
}

[ProtoPackable]
internal partial class FetchGroupExtraRequestConfigFlags
{
    [ProtoMember(22)]
    public bool LatestMessageSequence { get; set; }
}

[ProtoPackable]
internal partial class FetchGroupExtraResponse
{
    [ProtoMember(1)]
    public FetchGroupExtraResponseInfo Info { get; set; }
}

[ProtoPackable]
internal partial class FetchGroupExtraResponseInfo
{
    [ProtoMember(1)]
    public long GroupUin { get; set; }

    [ProtoMember(3)]
    public FetchGroupExtraResponseInfoResult Result { get; set; }
}

[ProtoPackable]
internal partial class FetchGroupExtraResponseInfoResult
{
    [ProtoMember(22)]
    public long LatestMessageSequence { get; set; }
}