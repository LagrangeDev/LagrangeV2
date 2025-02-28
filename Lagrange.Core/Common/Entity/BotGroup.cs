namespace Lagrange.Core.Common.Entity;

public class BotGroup(
    long groupUin,
    string groupName,
    int memberCount,
    int maxMember,
    long createTime,
    string? description,
    string? question,
    string? announcement)
{
    public long GroupUin { get; } = groupUin;

    public string GroupName { get; } = groupName;

    public int MemberCount { get; } = memberCount;

    public int MaxMember { get; } = maxMember;

    public long CreateTime { get; } = createTime;

    public string? Description { get; } = description;

    public string? Question { get; } = question;

    public string? Announcement { get; } = announcement;
}