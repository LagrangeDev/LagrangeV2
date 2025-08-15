using Lagrange.Core.Common;
using Lagrange.Core.Internal.Events;
using Lagrange.Core.Internal.Events.System;
using Lagrange.Core.Internal.Packets.Service;

namespace Lagrange.Core.Internal.Services.System;

[EventSubscribe<SetGroupNotificationEventReq>(Protocols.All)]
[Service("OidbSvcTrpcTcp.0x10c8_1")]
internal class SetGroupNotificationService : OidbService<SetGroupNotificationEventReq, SetGroupNotificationEventResp, SetGroupNotificationRequest, SetGroupNotificationResponse>
{
    private protected override uint Command => 0x10c8;

    private protected override uint Service => 1;

    private protected override Task<SetGroupNotificationRequest> ProcessRequest(SetGroupNotificationEventReq request, BotContext context)
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

    private protected override Task<SetGroupNotificationEventResp> ProcessResponse(SetGroupNotificationResponse response, BotContext context)
    {
        return Task.FromResult(SetGroupNotificationEventResp.Default);
    }
}
