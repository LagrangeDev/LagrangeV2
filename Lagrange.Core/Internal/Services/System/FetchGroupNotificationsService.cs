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
            var target = context.CacheContext.ResolveUin(request.Target.Uid);
            long? @operator = request.Operator != null
                ? context.CacheContext.ResolveUin(request.Operator.Uid)
                : null;
            long? inviter = request.Inviter != null
                ? context.CacheContext.ResolveUin(request.Inviter.Uid)
                : null;

            notifications.Add(request.Type switch
            {
                1 => new BotGroupJoinNotification(
                    request.Group.GroupUin,
                    request.Sequence,
                    target,
                    (BotGroupNotificationState)request.State,
                    @operator,
                    request.Comment
                ),
                6 or 7 => new BotGroupKickNotification(
                    request.Group.GroupUin,
                    request.Sequence,
                    target,
                    @operator ?? 0
                ),
                13 => new BotGroupExitNotification(
                    request.Group.GroupUin,
                    request.Sequence,
                    target
                ),
                22 => new BotGroupInviteNotification(
                    request.Group.GroupUin,
                    request.Sequence,
                    target,
                    (BotGroupNotificationState)request.State,
                    @operator,
                    inviter ?? 0
                ),
                _ => throw new NotImplementedException(),
            });
        }
        return Task.FromResult(new FetchGroupNotificationsEventResp(notifications));
    }
}