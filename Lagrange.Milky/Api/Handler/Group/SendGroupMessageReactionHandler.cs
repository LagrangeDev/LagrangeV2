
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
        // TODO: Core SetGroupReaction does not support reaction_type (emoji)
        await _bot.SetGroupReaction(parameter.GroupId, (ulong)parameter.MessageSeq, parameter.Reaction, parameter.IsAdd);
    }
}

public class SendGroupMessageReactionParameter(long groupId, long messageSeq, string reaction, string reactionType = "face", bool isAdd = true)
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

    [JsonPropertyName("reaction_type")]
    public string ReactionType { get; init; } = reactionType;

    [JsonPropertyName("is_add")]
    public bool IsAdd { get; init; } = isAdd;
}
