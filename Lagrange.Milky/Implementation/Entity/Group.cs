namespace Lagrange.Milky.Implementation.Entity;

public class Group
{
    public required long GroupId { get; init; }
    public required string Name { get; init; }
    public required int MemberCount { get; init; }
    public required int MaxMemberCount { get; init; }
}