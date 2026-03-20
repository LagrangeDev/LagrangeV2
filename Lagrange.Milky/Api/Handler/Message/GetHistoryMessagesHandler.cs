using System.Text.Json.Serialization;
using Lagrange.Core;
using Lagrange.Core.Common.Interface;
using Lagrange.Milky.Api.Exception;
using Lagrange.Milky.Entity.Message;
using Lagrange.Milky.Utility;

namespace Lagrange.Milky.Api.Handler.Message;

[Api("get_history_messages")]
public class GetHistoryMessagesHandler(BotContext bot, EntityConvert convert) : IApiHandler<GetHistoryMessagesParameter, GetHistoryMessagesResult>
{
    private readonly BotContext _bot = bot;
    private readonly EntityConvert _convert = convert;

    public async Task<GetHistoryMessagesResult> HandleAsync(GetHistoryMessagesParameter parameter, CancellationToken token)
    {
        int start = parameter.StartMessageSeq.HasValue
            ? (int)(parameter.StartMessageSeq.Value - parameter.Limit)
            : parameter.MessageScene switch
            {
                "group" => (int)(await _bot.FetchGroupExtra(parameter.PeerId)).LatestMessageSequence,
                "friend" => throw new NotImplementedException(),
                "temp" => throw new ApiException(-1, "tmp will not be implemented."),
                _ => throw new NotSupportedException(),
            };
        int end = start + parameter.Limit;

        var messages = parameter.MessageScene switch
        {
            "friend" => await _bot.GetC2CMessage(parameter.PeerId, (ulong)start, (ulong)end),
            "group" => await _bot.GetGroupMessage(parameter.PeerId, (ulong)start, (ulong)end),
            "temp" => throw new ApiException(-1, "tmp will not be implemented."),
            _ => throw new NotSupportedException(),
        };

        return new GetHistoryMessagesResult(messages.Select(_convert.MessageBase), start - 1 > 0 ? start - 1 : null);
    }
}

public class GetHistoryMessagesParameter(string messageScene, long peerId, long? startMessageSeq, int limit = 20)
{
    [JsonRequired]
    [JsonPropertyName("message_scene")]
    public string MessageScene { get; init; } = messageScene;

    [JsonRequired]
    [JsonPropertyName("peer_id")]
    public long PeerId { get; init; } = peerId;

    [JsonPropertyName("start_message_seq")]
    public long? StartMessageSeq { get; } = startMessageSeq;

    [JsonPropertyName("limit")]
    public int Limit { get; } = limit;
}

public class GetHistoryMessagesResult(IEnumerable<MessageBase> messages, long? nextMessageSeq)
{
    [JsonPropertyName("messages")]
    public IEnumerable<MessageBase> Messages { get; init; } = messages;

    [JsonPropertyName("next_message_seq")]
    public long? NextMessageSeq { get; init; } = nextMessageSeq;
}