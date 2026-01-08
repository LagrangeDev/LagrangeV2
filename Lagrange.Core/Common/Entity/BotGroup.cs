namespace Lagrange.Core.Common.Entity;

public class BotGroup(
    long groupUin,
    string groupName,
    int memberCount,
    int maxMember,
    long createTime,
    string? description,
    string? question,
    string? announcement,
    uint lastestSeq = 0) : BotContact
{
    public long GroupUin { get; } = groupUin;

    public string GroupName { get; } = groupName ?? string.Empty;

    public int MemberCount { get; } = memberCount;

    public int MaxMember { get; } = maxMember;

    public long CreateTime { get; } = createTime;

    public string? Description { get; } = description;

    public string? Question { get; } = question;

    public string? Announcement { get; } = announcement;

    /// <summary>
    /// The latest message sequence number for this group.
    /// </summary>
    public uint LastestSeq { get; } = lastestSeq;
    
    public override long Uin => GroupUin;

    public override string Nickname => GroupName;

    public override string Uid => GroupUin.ToString();
}