using System.Text.Json.Serialization;
using Lagrange.Core;
using Lagrange.Core.Common.Interface;
using Lagrange.Milky.Implementation.Common.Api.Params;
using Lagrange.Milky.Implementation.Common.Api.Results;
using Lagrange.Milky.Implementation.Entity;

namespace Lagrange.Milky.Implementation.Api.System;

[Api("get_friend_info")]
public class GetFriendInfoHandler(BotContext bot) : IApiHandler<GetFriendInfoParam>
{
    public async ValueTask<IApiResult> HandleAsync(GetFriendInfoParam param, CancellationToken token)
    {
        var friend = (await bot.FetchFriends(param.NoCache ?? false))
            .FirstOrDefault(friend => friend.Uin == param.UserId);

        if (friend == null) return new ApiFailedResult
        {
            Retcode = -1,
            Message = "friend not found",
        };

        return new ApiOkResult<Friend>
        {
            Data = new Friend
            {
                UserId = friend.Uin,
                Qid = friend.Qid,
                Nickname = friend.Nickname,
                Remark = friend.Remarks,
                Category = new FriendCategory
                {
                    CategoryId = friend.Category.Id,
                    CategoryName = friend.Category.Name,
                },
            },
        };
    }
}

public class GetFriendInfoParam : CachedParam
{
    [JsonPropertyName("user_id")]
    public required long UserId { get; init; }
}