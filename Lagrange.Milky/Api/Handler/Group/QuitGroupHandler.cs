using System.Text.Json.Serialization;
using Lagrange.Core;
using Lagrange.Core.Common.Interface;

namespace Lagrange.Milky.Api.Handler.Group;

[Api("quit_group")]
public class QuitGroupHandler(BotContext bot) : IEmptyResultApiHandler<QuitGroupParameter>
{
    private readonly BotContext _bot = bot;

    public async Task HandleAsync(QuitGroupParameter parameter, CancellationToken token)
    {
        await _bot.GroupQuit(parameter.GroupId);
    }
}

public class QuitGroupParameter(long groupId)
{
    [JsonRequired]
    [JsonPropertyName("group_id")]
    public long GroupId { get; init; } = groupId;
}
