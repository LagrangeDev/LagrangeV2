using System.Text.Json.Serialization;
using Lagrange.Core;
using Lagrange.Core.Common;
using Lagrange.Core.Common.Entity;
using Lagrange.Core.Common.Interface;
using Lagrange.Milky.Implementation.Common.Api.Params;
using Lagrange.Milky.Implementation.Common.Api.Results;
using Lagrange.Milky.Implementation.Entity;

namespace Lagrange.Milky.Implementation.Api.System;

[Api("get_group_member_list")]
public class GetGroupMemberListHandler(BotContext bot) : IApiHandler<GetGroupMemberListParam>
{
    public async ValueTask<IApiResult> HandleAsync(GetGroupMemberListParam param, CancellationToken token)
    {
        var members = (await bot.FetchMembers(param.GroupId, param.NoCache ?? false))
            .Select(member => new GroupMember
            {
                GroupId = member.Group.GroupUin,
                UserId = member.Uin,
                Nickname = member.Nickname,
                Card = member.MemberCard ?? string.Empty,
                Title = member.SpecialTitle,
                Sex = member.Gender switch
                {
                    BotGender.Male => "male",
                    BotGender.Female => "female",
                    _ => "unknown"
                },
                Level = member.GroupLevel,
                Role = member.Permission switch
                {
                    GroupMemberPermission.Member => "member",
                    GroupMemberPermission.Admin => "admin",
                    GroupMemberPermission.Owner => "owner",
                    _ => throw new NotSupportedException(),
                },
                JoinTime = new DateTimeOffset(member.JoinTime).ToUnixTimeSeconds(),
                LastSentTime = new DateTimeOffset(member.LastMsgTime).ToUnixTimeSeconds(),
            });

        return new ApiOkResult<IEnumerable<GroupMember>>
        {
            Data = members,
        };
    }
}

public class GetGroupMemberListParam : CachedParam
{
    public required long GroupId { get; init; }
}