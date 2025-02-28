namespace Lagrange.Core.Common.Entity;

public class BotGroupMember(
    long uin,
    string uid,
    string nickname,
    GroupMemberPermission permission,
    int groupLevel,
    string? memberCard,
    string? specialTitle,
    DateTime joinTime,
    DateTime lastMsgTime,
    DateTime shutUpTimestamp) : BotContact
{
    public override long Uin { get; } = uin;

    public override string Uid { get; } = uid;

    public override string Nickname { get; } = nickname;

    public GroupMemberPermission Permission { get; } = permission;

    public int GroupLevel { get; } = groupLevel;

    public string? MemberCard { get; } = memberCard;

    public string? SpecialTitle { get; } = specialTitle;

    public DateTime JoinTime { get; } = joinTime;

    public DateTime LastMsgTime { get; } = lastMsgTime;

    public DateTime ShutUpTimestamp { get; } = shutUpTimestamp;
}

public enum GroupMemberPermission
{
    Member,
    Admin,
    Owner
}