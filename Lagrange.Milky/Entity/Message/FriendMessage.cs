using System.Text.Json.Serialization;
using Lagrange.Milky.Entity.Segment;

namespace Lagrange.Milky.Entity.Message;

public class FriendMessage(long peerId, long messageSeq, long senderId, long time, IReadOnlyList<IIncomingSegment> segments, Friend friend) : MessageBase(peerId, messageSeq, senderId, time, segments, "friend")
{
    [JsonPropertyName("friend")]
    public Friend Friend { get; } = friend;
}