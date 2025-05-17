using System.Text.Json.Serialization;
using Lagrange.Core;
using Lagrange.Core.Common.Interface;
using Lagrange.Milky.Implementation.Common.Api.Params;
using Lagrange.Milky.Implementation.Common.Api.Results;
using Lagrange.Milky.Implementation.Entity;

namespace Lagrange.Milky.Implementation.Api.System;

[Api("get_group_info")]
public class GetGroupInfoHandler(BotContext bot) : IApiHandler<CachedParam>
{
    public async ValueTask<IApiResult> HandleAsync(CachedParam param, CancellationToken token)
    {
        var group = (await bot.FetchGroups(param.NoCache ?? false))
            .FirstOrDefault();

        if (group == null) return new ApiFailedResult
        {
            Retcode = -1,
            Message = "Group not found",
        };

        return new ApiOkResult<Group>
        {
            Data = new Group
            {
                GroupId = group.GroupUin,
                Name = group.GroupName,
                MemberCount = group.MemberCount,
                MaxMemberCount = group.MaxMember,
            },
        };
    }
}

public class GetGroupInfoParam : CachedParam
{
    [JsonPropertyName("group_id")]
    public required long GroupId { get; init; }
}