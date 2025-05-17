namespace Lagrange.Milky.Implementation.Entity;

public class Friend
{
    public required long UserId { get; init; }
    public required string Qid { get; init; }
    public required string Nickname { get; init; }
    public required string Remark { get; init; }
    public required FriendCategory Category { get; init; }
}