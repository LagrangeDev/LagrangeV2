using System.Text.Json.Serialization;
using Lagrange.Core;
using Lagrange.Core.Common.Interface;
using Lagrange.Milky.Entity.Segment;
using Lagrange.Milky.Utility;

namespace Lagrange.Milky.Api.Handler.Message;

[Api("send_private_message")]
public class SendPrivateMessageHandler(BotContext bot, EntityConvert convert) : IApiHandler<SendPrivateMessageParameter, SendPrivateMessageResult>
{
    private readonly BotContext _bot = bot;
    private readonly EntityConvert _convert = convert;

    public async Task<SendPrivateMessageResult> HandleAsync(SendPrivateMessageParameter parameter, CancellationToken token)
    {
        var chain = await _convert.FriendSegmentsAsync(parameter.Message, parameter.UserId, token);
        var result = await _bot.SendFriendMessage(parameter.UserId, chain);

        return new SendPrivateMessageResult(
            (long)result.Sequence,
            new DateTimeOffset(result.Time).ToUnixTimeSeconds()
        );
    }
}

public class SendPrivateMessageParameter(long userId, IReadOnlyList<IOutgoingSegment> message)
{
    [JsonRequired]
    [JsonPropertyName("user_id")]
    public long UserId { get; init; } = userId;

    [JsonRequired]
    [JsonPropertyName("message")]
    public IReadOnlyList<IOutgoingSegment> Message { get; init; } = message;
}

public class SendPrivateMessageResult(long messageSeq, long time)
{
    [JsonPropertyName("message_seq")]
    public long MessageSeq { get; } = messageSeq;

    [JsonPropertyName("time")]
    public long Time { get; } = time;
}