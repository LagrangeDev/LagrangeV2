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
        int end;

        if (parameter.StartMessageSeq.HasValue)
        {
            start = (int)(parameter.StartMessageSeq.Value - parameter.Limit);
            end = (int)parameter.StartMessageSeq.Value;
        }
        else
        {
            // No start sequence provided, try to get the latest sequence
            switch (parameter.MessageScene)
            {
                case "group":
                    var groups = await _bot.FetchGroups();
                    var group = groups.FirstOrDefault(g => g.GroupUin == parameter.PeerId)
                        ?? throw new ApiException(-1, $"Group {parameter.PeerId} not found");
                    
                    if (group.LastestSeq == 0)
                        throw new ApiException(-1, $"Failed to get latest sequence for group {parameter.PeerId}");
                    
                    end = (int)group.LastestSeq;
                    start = end - parameter.Limit;
                    break;
                    
                case "friend":
                    throw new ApiException(-1, "Getting latest messages for friends without start_message_seq is not supported");
                    
                default:
                    throw new NotSupportedException($"Message scene '{parameter.MessageScene}' is not supported");
            }
        }

        if (start < 0) start = 0;

        var messages = parameter.MessageScene switch
        {
            "friend" => await _bot.GetC2CMessage(parameter.PeerId, (ulong)start, (ulong)end),
            "group" => await _bot.GetGroupMessage(parameter.PeerId, (ulong)start, (ulong)end),
            "temp" => throw new ApiException(-1, "temp not supported"),
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