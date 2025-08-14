using Lagrange.Core.Common;
using Lagrange.Core.Common.Entity;
using Lagrange.Core.Internal.Events;
using Lagrange.Core.Internal.Events.System;
using Lagrange.Core.Internal.Packets.Service;

namespace Lagrange.Core.Internal.Services.System;

[EventSubscribe<FetchGroupNotificationsEventReq>(Protocols.All)]
[Service("OidbSvcTrpcTcp.0x10c0_1")]
internal class FetchGroupNotificationsService : OidbService<FetchGroupNotificationsEventReq, FetchGroupNotificationsEventResp, FetchGroupNotificationsRequest, FetchGroupNotificationsResponse>
{
    private protected override uint Command => 0x10c0;

    private protected override uint Service => 1;

    private protected override Task<FetchGroupNotificationsRequest> ProcessRequest(FetchGroupNotificationsEventReq request, BotContext context)
    {
        return Task.FromResult(new FetchGroupNotificationsRequest
        {
            Count = request.Count
        });
    }

    private protected override Task<FetchGroupNotificationsEventResp> ProcessResponse(FetchGroupNotificationsResponse response, BotContext context)
    {
        List<BotGroupNotificationBase> notifications = [];
        foreach (var request in response.GroupNotifications)
        {
            var targetUin = context.CacheContext.ResolveUin(request.Target.Uid);
            long? operatorUin = request.Operator != null
                ? context.CacheContext.ResolveUin(request.Operator.Uid)
                : null;
            long? inviterUin = request.Inviter != null
                ? context.CacheContext.ResolveUin(request.Inviter.Uid)
                : null;

            var notification = request.Type switch
            {
                1 => new BotGroupJoinNotification(
                    request.Group.GroupUin,
                    request.Sequence,
                    targetUin,
                    request.Target.Uid,
                    (BotGroupNotificationState)request.State,
                    operatorUin,
                    request.Operator?.Uid,
                    request.Comment
                ),
                3 => new BotGroupSetAdminNotification(
                    request.Group.GroupUin,
                    request.Sequence,
                    targetUin,
                    request.Target.Uid,
                    operatorUin ?? 0,
                    request.Operator?.Uid ?? string.Empty
                ),
                6 or 7 => new BotGroupKickNotification(
                    request.Group.GroupUin,
                    request.Sequence,
                    targetUin,
                    request.Target.Uid,
                    operatorUin ?? 0,
                    request.Operator?.Uid ?? string.Empty
                ),
                13 => new BotGroupExitNotification(
                    request.Group.GroupUin,
                    request.Sequence,
                    targetUin,
                    request.Target.Uid
                ),
                16 => new BotGroupUnsetAdminNotification(
                    request.Group.GroupUin,
                    request.Sequence,
                    targetUin,
                    request.Target.Uid,
                    operatorUin ?? 0,
                    request.Operator?.Uid ?? string.Empty
                ),
                22 => new BotGroupInviteNotification(
                    request.Group.GroupUin,
                    request.Sequence,
                    targetUin,
                    request.Target.Uid,
                    (BotGroupNotificationState)request.State,
                    operatorUin,
                    request.Operator?.Uid,
                    inviterUin ?? 0,
                    request.Inviter?.Uid ?? string.Empty
                ),
                _ => LogUnknownNotificationType(context, request.Type),
            };
            if (notification != null) notifications.Add(notification);
        }
        return Task.FromResult(new FetchGroupNotificationsEventResp(notifications));
    }

    private BotGroupNotificationBase? LogUnknownNotificationType(BotContext context, ulong type)
    {
        context.LogWarning(nameof(FetchGroupNotificationsService), "Unknown notification type: {0}", null, type);
        return null;
    }
}