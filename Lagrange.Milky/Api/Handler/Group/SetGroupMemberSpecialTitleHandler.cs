using System.Text.Json.Serialization;
using Lagrange.Core;
using Lagrange.Core.Common.Interface;

namespace Lagrange.Milky.Api.Handler.Group;

[Api("set_group_member_special_title")]
public class SetGroupMemberSpecialTitleHandler(BotContext bot) : IEmptyResultApiHandler<SetGroupMemberSpecialTitleParameter>
{
    private readonly BotContext _bot = bot;

    public async Task HandleAsync(SetGroupMemberSpecialTitleParameter parameter, CancellationToken token)
    {
        await _bot.GroupSetSpecialTitle(parameter.GroupId, parameter.UserId, parameter.SpecialTitle);
    }
}

public class SetGroupMemberSpecialTitleParameter(long groupId, long userId, string specialTitle)
{
    [JsonRequired]
    [JsonPropertyName("group_id")]
    public long GroupId { get; init; } = groupId;

    [JsonRequired]
    [JsonPropertyName("user_id")]
    public long UserId { get; init; } = userId;

    [JsonRequired]
    [JsonPropertyName("special_title")]
    public string SpecialTitle { get; init; } = specialTitle;
}
