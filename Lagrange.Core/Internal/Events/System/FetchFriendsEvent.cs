using Lagrange.Core.Common.Entity;

namespace Lagrange.Core.Internal.Events.System;

internal class FetchFriendsEventReq(byte[]? cookie) : ProtocolEvent
{
    public byte[]? Cookie { get; set; } = cookie; // for the request of next page
}

internal class FetchFriendsEventResp(List<BotFriend> friends, List<BotFriendCategory> categories, byte[]? cookie) : ProtocolEvent
{
    public List<BotFriend> Friends { get; } = friends;

    public List<BotFriendCategory> Categories { get; } = categories;
    
    public byte[]? Cookie { get; } = cookie;
}