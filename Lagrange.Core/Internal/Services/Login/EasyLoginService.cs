using Lagrange.Core.Common;
using Lagrange.Core.Internal.Events;
using Lagrange.Core.Internal.Events.Login;
using Lagrange.Core.Internal.Packets.Login;
using Lagrange.Core.Internal.Packets.Struct;

namespace Lagrange.Core.Internal.Services.Login;

[EventSubscribe<EasyLoginEventReq>(Protocols.PC)]
[Service("trpc.login.ecdh.EcdhService.SsoNTLoginEasyLogin", RequestType.D2Auth, EncryptType.EncryptEmpty)]
internal class EasyLoginService : BaseService<EasyLoginEventReq, EasyLoginEventResp>
{
    protected override ValueTask<ReadOnlyMemory<byte>> Build(EasyLoginEventReq input, BotContext context)
    {
        if (context.Keystore.WLoginSigs.A1 is not { Length: > 0 } a1) throw new InvalidOperationException("A1 is not set");
        
        var reqBody = new NTLoginEasyLoginReqBody { A1 = a1 };

        return new ValueTask<ReadOnlyMemory<byte>>(NTLoginCommon.Encode(context, reqBody));
    }

    protected override ValueTask<EasyLoginEventResp> Parse(ReadOnlyMemory<byte> input, BotContext context)
    {
        var state = NTLoginCommon.Decode<NTLoginEasyLoginRspBody>(context, input, out var info, out var resp);
        if (state == NTLoginRetCode.SUCCESS_UNSPECIFIED) NTLoginCommon.SaveTicket(context, resp.Tickets);
        
        return new ValueTask<EasyLoginEventResp>(state switch
        {
            NTLoginRetCode.SUCCESS_UNSPECIFIED => new EasyLoginEventResp(state, null, null),
            NTLoginRetCode.ERR_NEED_VERIFY_UNUSUAL_DEVICE => new EasyLoginEventResp(state, null, resp.SecProtect.UnusualDeviceCheckSig),
            _ when info is not null => new EasyLoginEventResp(state, (info.StrTipsTitle, info.StrTipsContent), null),
            _ => new EasyLoginEventResp(state, null, null)
        });
    }
}