using Lagrange.Core.Common;
using Lagrange.Core.Internal.Events;
using Lagrange.Core.Internal.Events.System;
using Lagrange.Core.Internal.Packets.Service;

namespace Lagrange.Core.Internal.Services.System;

[EventSubscribe<ReduceGroupReactionEventReq>(Protocols.All)]
[Service("OidbSvcTrpcTcp.0x9082_2")]
internal class ReduceGroupReactionService : OidbService<ReduceGroupReactionEventReq, ReduceGroupReactionEventResp, SetGroupReactionRequest, SetGroupReactionResponse>
{
    private protected override uint Command => 0x9082;

    private protected override uint Service => 2;

    private protected override Task<SetGroupReactionRequest> ProcessRequest(ReduceGroupReactionEventReq request, BotContext context)
    {
        return Task.FromResult(new SetGroupReactionRequest
        {
            GroupUin = request.GroupUin,
            Sequence = request.Sequence,
            Code = request.Code,
            Type = request.Code.Length <= 3 ? 1ul : 2ul
        });
    }

    private protected override Task<ReduceGroupReactionEventResp> ProcessResponse(SetGroupReactionResponse response, BotContext context)
    {
        return Task.FromResult(ReduceGroupReactionEventResp.Default);
    }
}
