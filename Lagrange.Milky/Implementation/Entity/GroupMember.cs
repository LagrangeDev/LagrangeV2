namespace Lagrange.Milky.Implementation.Entity;

public class GroupMember
{
    public required long GroupId { get; init; }
    public required long UserId { get; init; }
    public required string Nickname { get; init; }
    public required string Card { get; init; }
    public required string? Title { get; init; }
    public required string Sex { get; init; }
    public required int Level { get; init; }
    public required string Role { get; init; }
    public required long JoinTime { get; init; }
    public required long LastSentTime { get; init; }
}