using System.Text.Json.Serialization;
using Lagrange.Core;
using Lagrange.Milky.Api.Exception;
using Lagrange.Milky.Cache;
using Lagrange.Milky.Entity.Message;
using Lagrange.Milky.Utility;

namespace Lagrange.Milky.Api.Handler.Message;

[Api("get_message")]
public class GetMessageHandler(BotContext bot, MessageCache cache, EntityConvert convert) : IApiHandler<GetMessageParameter, GetMessageResult>
{
    private readonly BotContext _bot = bot;
    private readonly MessageCache _cache = cache;
    private readonly EntityConvert _convert = convert;

    public async Task<GetMessageResult> HandleAsync(GetMessageParameter parameter, CancellationToken token)
    {
        var message = await _cache.GetMessageAsync(
            parameter.MessageScene switch
            {
                "friend" => Lagrange.Core.Message.MessageType.Private,
                "group" => Lagrange.Core.Message.MessageType.Group,
                "temp" => throw new ApiException(-1, "temp not supported"),
                _ => throw new NotSupportedException(),
            },
            parameter.PeerId,
            (ulong)parameter.MessageSeq,
            token
        );

        if (message == null) throw new ApiException(-2, "message not found");

        return new GetMessageResult(_convert.MessageBase(message));
    }
}

public class GetMessageParameter(string messageScene, long peerId, long messageSeq)
{
    [JsonRequired]
    [JsonPropertyName("message_scene")]
    public string MessageScene { get; init; } = messageScene;

    [JsonRequired]
    [JsonPropertyName("peer_id")]
    public long PeerId { get; init; } = peerId;

    [JsonRequired]
    [JsonPropertyName("message_seq")]
    public long MessageSeq { get; init; } = messageSeq;
}

public class GetMessageResult(MessageBase message)
{
    [JsonPropertyName("message")]
    public MessageBase Message { get; init; } = message;
}