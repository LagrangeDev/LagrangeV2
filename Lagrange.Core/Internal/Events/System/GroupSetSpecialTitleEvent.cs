namespace Lagrange.Core.Internal.Events.System;

internal class GroupSetSpecialTitleEventReq(long groupUin, string targetUid, string title, uint expireTime) : ProtocolEvent
{
    public long GroupUin { get; } = groupUin;

    public string TargetUid { get; } = targetUid;

    public string Title { get; } = title;

    public uint ExpireTime { get; } = expireTime;
}

internal class GroupSetSpecialTitleEventResp : ProtocolEvent
{
    public static readonly GroupSetSpecialTitleEventResp Default = new();
}
