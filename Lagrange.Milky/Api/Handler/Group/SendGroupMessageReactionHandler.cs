
using System.Text.Json.Serialization;
using Lagrange.Core;
using Lagrange.Core.Common.Interface;

namespace Lagrange.Milky.Api.Handler.Group;

[Api("send_group_message_reaction")]
public class SendGroupMessageReactionHandler(BotContext bot) : IEmptyResultApiHandler<SendGroupMessageReactionParameter>
{
    private readonly BotContext _bot = bot;

    public async Task HandleAsync(SendGroupMessageReactionParameter parameter, CancellationToken token)
    {
        await _bot.SetGroupReaction(parameter.GroupId, (ulong)parameter.MessageSeq, parameter.Reaction, true);
    }
}

public class SendGroupMessageReactionParameter(long groupId, long messageSeq, string reaction, bool isAdd)
{
    [JsonRequired]
    [JsonPropertyName("group_id")]
    public long GroupId { get; init; } = groupId;

    [JsonRequired]
    [JsonPropertyName("message_seq")]
    public long MessageSeq { get; init; } = messageSeq;

    [JsonRequired]
    [JsonPropertyName("reaction")]
    public string Reaction { get; init; } = reaction;

    [JsonRequired]
    [JsonPropertyName("is_add")]
    public bool IsAdd { get; init; } = isAdd;
}
