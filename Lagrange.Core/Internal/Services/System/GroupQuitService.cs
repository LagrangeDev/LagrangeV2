﻿using Lagrange.Core.Common;
using Lagrange.Core.Internal.Events;
using Lagrange.Core.Internal.Events.System;
using Lagrange.Core.Internal.Packets.Service;

namespace Lagrange.Core.Internal.Services.System;

[EventSubscribe<GroupQuitEventReq>(Protocols.All)]
[Service("OidbSvcTrpcTcp.0x1097_1")]
internal class GroupQuitService: OidbService<GroupQuitEventReq, GroupQuitEventResp, D1097ReqBody, D1097RspBody>
{
    private protected override uint Command => 0x1097;

    private protected override uint Service => 1;

    private protected override Task<D1097ReqBody> ProcessRequest(GroupQuitEventReq request, BotContext context)
    {
        return Task.FromResult(new D1097ReqBody
        {
            GroupCode = request.GroupUin
        });
    }

    private protected override Task<GroupQuitEventResp> ProcessResponse(D1097RspBody response, BotContext context)
    {
        return Task.FromResult(GroupQuitEventResp.Default);
    }
}
