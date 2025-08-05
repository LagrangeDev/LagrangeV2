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
        int start;
        if (parameter.StartMessageSeq.HasValue) start = (int)(parameter.StartMessageSeq.Value - parameter.Limit);
        // TODO: No start sequence
        else throw new NotImplementedException();

        int end = start + parameter.Limit;

        var messages = parameter.MessageScene switch
        {
            "friend" => await _bot.GetC2CMessage(parameter.PeerId, start, end),
            "group" => await _bot.GetGroupMessage(parameter.PeerId, start, end),
            "temp" => throw new ApiException(-1, "temp not supported"),
            _ => throw new NotSupportedException(),
        };

        return new GetHistoryMessagesResult(messages.Select(_convert.MessageBase));
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

public class GetHistoryMessagesResult(IEnumerable<MessageBase> messages)
{
    [JsonPropertyName("messages")]
    public IEnumerable<MessageBase> Messages { get; init; } = messages;
}