using Lagrange.Proto;

namespace Lagrange.Core.Internal.Packets.Service;

#pragma warning disable CS8618

[ProtoPackable]
internal partial class GroupRenameReqBody
{
    [ProtoMember(3)] public string TargetName { get; set; }
}

[ProtoPackable]
internal partial class D89AReqBody
{
    [ProtoMember(1)] public long GroupUin { get; set; }

    [ProtoMember(2)] public GroupRenameReqBody Body { get; set; }
}

[ProtoPackable]
internal partial class D89ARspBody;
