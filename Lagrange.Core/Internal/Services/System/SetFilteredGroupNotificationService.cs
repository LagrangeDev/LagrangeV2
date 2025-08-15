using Lagrange.Core.Common;
using Lagrange.Core.Internal.Events;
using Lagrange.Core.Internal.Events.System;
using Lagrange.Core.Internal.Packets.Service;

namespace Lagrange.Core.Internal.Services.System;

[EventSubscribe<SetFilteredGroupNotificationEventReq>(Protocols.All)]
[Service("OidbSvcTrpcTcp.0x10c8_2")]
internal class SetFilteredGroupNotificationService : OidbService<SetFilteredGroupNotificationEventReq, SetFilteredGroupNotificationEventResp, SetGroupNotificationRequest, SetGroupNotificationResponse>
{
    private protected override uint Command => 0x10c8;

    private protected override uint Service => 2;

    private protected override Task<SetGroupNotificationRequest> ProcessRequest(SetFilteredGroupNotificationEventReq request, BotContext context)
    {
        return Task.FromResult(new SetGroupNotificationRequest
        {
            Operate = (ulong)request.Operate,
            Body = new SetGroupNotificationRequestBody
            {
                Sequence = request.Sequence,
                Type = (ulong)request.Type,
                GroupUin = request.GroupUin,
                Message = request.Message,
            }
        });
    }

    private protected override Task<SetFilteredGroupNotificationEventResp> ProcessResponse(SetGroupNotificationResponse response, BotContext context)
    {
        return Task.FromResult(SetFilteredGroupNotificationEventResp.Default);
    }
}
