using Lagrange.Core.Common;
using Lagrange.Core.Internal.Events;
using Lagrange.Core.Internal.Events.Login;
using Lagrange.Core.Internal.Packets.Login;

namespace Lagrange.Core.Internal.Services.Login;

[EventSubscribe<RefreshTicketEventReq>(Protocols.Android)]
[Service("trpc.login.ecdh.EcdhService.SsoNTLoginRefreshTicket")]
internal class RefreshTicketService : BaseService<RefreshTicketEventReq, RefreshTicketEventResp>
{
    protected override ValueTask<ReadOnlyMemory<byte>> Build(RefreshTicketEventReq input, BotContext context)
    {
        if (context.Keystore.WLoginSigs.A1 is not { Length: > 0 } a1) throw new InvalidOperationException("A1 is not set");

        var reqBody = new NTLoginRefreshTicketReqBody { A1 = a1 };
        return ValueTask.FromResult(NTLoginCommon.Encode(context, reqBody));
    }

    protected override ValueTask<RefreshTicketEventResp> Parse(ReadOnlyMemory<byte> input, BotContext context)
    {
        var state = NTLoginCommon.Decode<NTLoginRefreshTicketRspBody>(context, input, out var info, out var resp);
        if (state == NTLoginRetCode.SUCCESS_UNSPECIFIED) NTLoginCommon.SaveTicket(context, resp.Tickets);

        return new ValueTask<RefreshTicketEventResp>(state switch
        {
            NTLoginRetCode.SUCCESS_UNSPECIFIED => new RefreshTicketEventResp(state, null),
            _ when info is not null => new RefreshTicketEventResp(state, (info.StrTipsTitle, info.StrTipsContent)),
            _ => new RefreshTicketEventResp(state, null)
        });
    }
}