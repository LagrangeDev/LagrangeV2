using System.Text.Json.Serialization;
using Lagrange.Core;
using Lagrange.Core.Common.Interface;

namespace Lagrange.Milky.Api.Handler.Group;

[Api("set_group_name")]
public class SetGroupNameHandler(BotContext bot) : IEmptyResultApiHandler<SetGroupNameParameter>
{
    private readonly BotContext _bot = bot;

    public async Task HandleAsync(SetGroupNameParameter parameter, CancellationToken token)
    {
        await _bot.GroupRename(parameter.GroupId, parameter.Name);
    }
}

public class SetGroupNameParameter(long groupId, string name)
{
    [JsonRequired]
    [JsonPropertyName("group_id")]
    public long GroupId { get; init; } = groupId;

    [JsonRequired]
    [JsonPropertyName("name")]
    public string Name { get; init; } = name;
}
