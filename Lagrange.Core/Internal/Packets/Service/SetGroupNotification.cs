using Lagrange.Proto;

namespace Lagrange.Core.Internal.Packets.Service;

#pragma warning disable CS8618

[ProtoPackable]
public partial class SetGroupNotificationRequest
{
    [ProtoMember(1)] public ulong Operate { get; set; } // 1 accept 2 reject 3 ignore

    [ProtoMember(2)] public SetGroupNotificationRequestBody Body { get; set; }
}

[ProtoPackable]
public partial class SetGroupNotificationRequestBody
{
    [ProtoMember(1)] public ulong Sequence { get; set; }

    [ProtoMember(2)] public ulong Type { get; set; }

    [ProtoMember(3)] public long GroupUin { get; set; }

    [ProtoMember(4)] public string Message { get; set; }
}

[ProtoPackable]
public partial class SetGroupNotificationResponse
{

}