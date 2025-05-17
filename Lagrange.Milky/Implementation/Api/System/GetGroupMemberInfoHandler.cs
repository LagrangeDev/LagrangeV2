using System.Text.Json.Serialization;
using Lagrange.Core;
using Lagrange.Core.Common;
using Lagrange.Core.Common.Entity;
using Lagrange.Core.Common.Interface;
using Lagrange.Milky.Implementation.Common.Api.Params;
using Lagrange.Milky.Implementation.Common.Api.Results;
using Lagrange.Milky.Implementation.Entity;

namespace Lagrange.Milky.Implementation.Api.System;

[Api("get_group_member_info")]
public class GetGroupMemberInfoHandler(BotContext bot) : IApiHandler<GetGroupMemberInfoParam>
{
    public async ValueTask<IApiResult> HandleAsync(GetGroupMemberInfoParam param, CancellationToken token)
    {
        var member = (await bot.FetchMembers(param.GroupId, param.NoCache ?? false))
            .FirstOrDefault(member => member.Uin == param.UserId);

        if (member == null) return new ApiFailedResult
        {
            Retcode = -1,
            Message = "Member not found",
        };

        return new ApiOkResult<GroupMember>
        {
            Data = new GroupMember
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
            },
        };
    }
}

public class GetGroupMemberInfoParam : CachedParam
{
    [JsonPropertyName("group_id")]
    public required long GroupId { get; init; }

    [JsonPropertyName("user_id")]
    public required long UserId { get; init; }
}