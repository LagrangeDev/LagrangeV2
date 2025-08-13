using Lagrange.Proto;

namespace Lagrange.Core.Internal.Packets.Service;

#pragma warning disable CS8618

[ProtoPackable]
internal partial class FetchStrangerByUidRequest
{
    [ProtoMember(1)] public string Uid { get; set; }

    [ProtoMember(3)] public List<FetchStrangerRequestKey> Keys { get; set; }
}

[ProtoPackable]
internal partial class FetchStrangerByUinRequest
{
    [ProtoMember(1)] public long Uin { get; set; }

    [ProtoMember(3)] public List<FetchStrangerRequestKey> Keys { get; set; }
}

[ProtoPackable]
internal partial class FetchStrangerRequestKey
{
    [ProtoMember(1)] public ulong Key { get; set; }
}


[ProtoPackable]
internal partial class FetchStrangerResponse
{
    [ProtoMember(1)] public FetchStrangerResponseBody Body { get; set; }
}

[ProtoPackable]
internal partial class FetchStrangerResponseBody
{
    [ProtoMember(2)] public FetchStrangerResponseProperties Properties { get; set; }

    [ProtoMember(3)] public uint Uin { get; set; }
}

[ProtoPackable]
public partial class FetchStrangerResponseProperties
{
    [ProtoMember(1)] public List<FetchStrangerResponseNumberProperties> NumberProperties { get; set; }

    [ProtoMember(2)] public List<FetchStrangerResponseBytesProperties> BytesProperties { get; set; }
}

[ProtoPackable]
public partial class FetchStrangerResponseNumberProperties
{
    [ProtoMember(1)] public ulong Key { get; set; }

    [ProtoMember(2)] public ulong Value { get; set; }
}

[ProtoPackable]
public partial class FetchStrangerResponseBytesProperties
{
    [ProtoMember(1)] public ulong Key { get; set; }

    [ProtoMember(2)] public byte[] Value { get; set; }
}