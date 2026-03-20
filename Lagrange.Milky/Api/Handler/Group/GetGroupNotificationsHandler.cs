using System.Text.Json.Serialization;
using Lagrange.Core;
using Lagrange.Core.Common.Entity;
using Lagrange.Core.Common.Interface;
using Lagrange.Milky.Entity;
using Lagrange.Milky.Utility;

namespace Lagrange.Milky.Api.Handler.Group;

[Api("get_group_notifications")]
public class GetGroupNotificationsHandler(BotContext bot, EntityConvert convert) : IApiHandler<GetGroupNotificationsParameter, GetGroupNotificationsResult>
{
    private readonly BotContext _bot = bot;
    private readonly EntityConvert _convert = convert;

    public async Task<GetGroupNotificationsResult> HandleAsync(GetGroupNotificationsParameter parameter, CancellationToken token)
    {
        var notifications = await (parameter.IsFiltered
            ? _bot.FetchFilteredGroupNotifications((ulong)parameter.Limit, (ulong)(parameter.StartNotificationSeq ?? 0))
            : _bot.FetchGroupNotifications((ulong)parameter.Limit, (ulong)(parameter.StartNotificationSeq ?? 0)));

        return new GetGroupNotificationsResult(
            notifications.Select(_convert.GroupNotification).Where(n => n != null)!,
            notifications.Count == 0 ? null : (long)notifications[^1].Sequence
        );
    }
}

public class GetGroupNotificationsParameter(long? startNotificationSeq, bool isFiltered = false, int limit = 20)
{
    [JsonPropertyName("start_notification_seq")]
    public long? StartNotificationSeq { get; } = startNotificationSeq;

    [JsonPropertyName("is_filtered")]
    public bool IsFiltered { get; } = isFiltered;

    [JsonPropertyName("limit")]
    public int Limit { get; } = limit;
}

public class GetGroupNotificationsResult(IEnumerable<GroupNotificationBase> notifications, long? nextNotificationSeq)
{
    [JsonPropertyName("notifications")]
    public IEnumerable<GroupNotificationBase> Notifications { get; } = notifications;

    [JsonPropertyName("next_notification_seq")]
    public long? NextNotificationSeq { get; } = nextNotificationSeq;
}