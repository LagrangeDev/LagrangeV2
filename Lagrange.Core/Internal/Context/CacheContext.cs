using System.Collections.Concurrent;
using Lagrange.Core.Common.Entity;
using Lagrange.Core.Internal.Events.System;

namespace Lagrange.Core.Internal.Context;

internal class CacheContext(BotContext context)
{
    private List<BotFriend>? _friends;

    private List<BotGroup>? _groups;

    private readonly ConcurrentDictionary<long, string> _uinToUid = new();

    private readonly ConcurrentDictionary<long, List<BotGroupMember>> _members = new();

    private readonly Dictionary<int, BotFriendCategory> _categories = new();
    
    public async Task<List<BotFriend>> GetFriendList(bool refresh = false)
    {
        if (refresh || _friends == null) Interlocked.Exchange(ref _friends, await FetchFriends());

        return _friends;
    }

    public async Task<List<BotGroup>> GetGroupList(bool refresh = false)
    {
        if (refresh) Interlocked.Exchange(ref _groups, await FetchGroups());
        Interlocked.CompareExchange(ref _groups, await FetchGroups(), null);

        return _groups;
    }

    public async Task<List<BotGroupMember>> GetMemberList(long groupUin, bool refresh = false)
    {
        if (refresh || !_members.TryGetValue(groupUin, out var members))
        {
            members = _members[groupUin] = await FetchGroupMembers(groupUin);
        }

        return members;
    }

    public async Task<List<BotFriendCategory>> GetCategories(bool refresh = false)
    {
        if (refresh || _categories.Count == 0) Interlocked.Exchange(ref _friends, await FetchFriends());

        return _categories.Values.ToList();
    }

    public async Task<BotFriend?> ResolveFriend(long uin)
    {
        if (_friends == null) Interlocked.Exchange(ref _friends, await FetchFriends());
        var friend = _friends?.FirstOrDefault(f => f.Uin == uin);

        if (friend == null)
        {
            _friends = Interlocked.Exchange(ref _friends, await FetchFriends());
            friend = _friends?.FirstOrDefault(f => f.Uin == uin);
        }

        return friend;
    }

    public async Task<(BotGroup, BotGroupMember)?> ResolveMember(long groupUin, long memberUin)
    {
        var group = await ResolveGroup(groupUin);
        if (group == null) return null;

        if (!_members.TryGetValue(groupUin, out var members))
        {
            members = _members[groupUin] = await FetchGroupMembers(groupUin);
        }
        var member = members.FirstOrDefault(m => m.Uin == memberUin);
        return member == null ? null : (group, member);
    }

    public async Task<BotGroup?> ResolveGroup(long groupUin)
    {
        if (_groups == null) Interlocked.Exchange(ref _groups, await FetchGroups());
        var group = _groups?.FirstOrDefault(f => f.GroupUin == groupUin);

        if (group == null)
        {
            _groups = Interlocked.Exchange(ref _groups, await FetchGroups());
            group = _groups?.FirstOrDefault(f => f.GroupUin == groupUin);
        }

        return group;
    }

    public string? ResolveCachedUid(long uin) => _uinToUid.GetValueOrDefault(uin);
    
    public long? ResolveCachedUin(string uid) => _uinToUid.FirstOrDefault(kvp => kvp.Value == uid).Key;

    /// <summary>
    /// Fetches the friends list from the server.
    /// </summary>
    private async Task<List<BotFriend>> FetchFriends()
    {
        var friends = new List<BotFriend>();

        byte[]? cookie = null;
        do
        {
            var result = await context.EventContext.SendEvent<FetchFriendsEventResp>(new FetchFriendsEventReq(cookie));
            cookie = result.Cookie;

            friends.AddRange(result.Friends);
            foreach (var category in result.Category) _categories[category.Id] = category;
            foreach (var friend in friends) _uinToUid[friend.Uin] = friend.Uid;
        } while (cookie != null);

        return friends;
    }

    private async Task<List<BotGroup>> FetchGroups()
    {
        var result = await context.EventContext.SendEvent<FetchGroupsEventResp>(new FetchGroupsEventReq());
        return result.Groups;
    }

    private async Task<List<BotGroupMember>> FetchGroupMembers(long groupUin)
    {
        var members = new List<BotGroupMember>();

        byte[]? cookie = null;
        do
        {
            var result = await context.EventContext.SendEvent<FetchGroupMembersEventResp>(new FetchGroupMembersEventReq(groupUin, cookie));
            cookie = result.Cookie;

            members.AddRange(result.GroupMembers);
            foreach (var member in result.GroupMembers) _uinToUid[member.Uin] = member.Uid;
        } while (cookie != null);

        return members;
    }
}