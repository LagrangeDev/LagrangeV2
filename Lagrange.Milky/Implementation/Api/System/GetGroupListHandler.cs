using Lagrange.Core;
using Lagrange.Core.Common.Interface;
using Lagrange.Milky.Implementation.Common.Api.Params;
using Lagrange.Milky.Implementation.Common.Api.Results;
using Lagrange.Milky.Implementation.Entity;

namespace Lagrange.Milky.Implementation.Api.System;

[Api("get_group_list")]
public class GetGroupListHandler(BotContext bot) : IApiHandler<CachedParam>
{
    public async ValueTask<IApiResult> HandleAsync(CachedParam param, CancellationToken token)
    {
        var groups = (await bot.FetchGroups(param.NoCache ?? false))
            .Select(group => new Group
            {
                GroupId = group.GroupUin,
                Name = group.GroupName,
                MemberCount = group.MemberCount,
                MaxMemberCount = group.MaxMember,
            });

        return new ApiOkResult<IEnumerable<Group>>
        {
            Data = groups,
        };
    }
}