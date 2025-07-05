using Lagrange.Proto;

namespace Lagrange.Core.Internal.Packets.Service;

#pragma warning disable CS8618

[ProtoPackable]
internal partial class D1097ReqBody
{
    [ProtoMember(1)] public long GroupCode { get; set; }
}

[ProtoPackable]
internal partial class D1097RspBody;
