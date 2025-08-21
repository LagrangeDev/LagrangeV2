using System.Text.Json.Serialization;
using Lagrange.Core;
using Lagrange.Core.Common.Interface;
using Lagrange.Milky.Entity.Segment;
using Lagrange.Milky.Utility;

namespace Lagrange.Milky.Api.Handler.Message;

[Api("send_group_message")]
public class SendGroupMessageHandler(BotContext bot, EntityConvert convert) : IApiHandler<SendGroupMessageParameter, SendGroupMessageResult>
{
    private readonly BotContext _bot = bot;
    private readonly EntityConvert _convert = convert;

    public async Task<SendGroupMessageResult> HandleAsync(SendGroupMessageParameter parameter, CancellationToken token)
    {
        var chain = await _convert.GroupSegmentsAsync(parameter.Message, parameter.GroupId, token);
        var result = await _bot.SendGroupMessage(parameter.GroupId, chain);

        return new SendGroupMessageResult((long)result.Sequence, new DateTimeOffset(result.Time).ToUnixTimeSeconds());
    }
}

public class SendGroupMessageParameter(long groupId, IReadOnlyList<IOutgoingSegment> message)
{
    [JsonRequired]
    [JsonPropertyName("group_id")]
    public long GroupId { get; init; } = groupId;

    [JsonRequired]
    [JsonPropertyName("message")]
    public IReadOnlyList<IOutgoingSegment> Message { get; init; } = message;
}

public class SendGroupMessageResult(long messageSeq, long time)
{
    [JsonPropertyName("message_seq")]
    public long MessageSeq { get; } = messageSeq;

    [JsonPropertyName("time")]
    public long Time { get; } = time;
}