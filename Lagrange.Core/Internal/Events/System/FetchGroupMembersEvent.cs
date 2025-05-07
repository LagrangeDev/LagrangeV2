using Lagrange.Core.Common.Entity;

namespace Lagrange.Core.Internal.Events.System;

internal class FetchGroupMembersEventReq(ulong groupUin, byte[]? cookie) : ProtocolEvent
{
    public ulong GroupUin { get; } = groupUin;

    public byte[]? Cookie { get; } = cookie;
}

internal class FetchGroupMembersEventResp(List<BotGroupMember> groupMembers, byte[]? cookie) : ProtocolEvent
{
    public List<BotGroupMember> GroupMembers { get; } = groupMembers;

    public byte[]? Cookie { get; } = cookie;
}