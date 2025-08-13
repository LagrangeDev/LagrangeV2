using Lagrange.Core.Exceptions;
using Lagrange.Core.Internal.Events;
using Lagrange.Core.Internal.Packets.Service;
using Lagrange.Core.Utility;
using Lagrange.Proto;

namespace Lagrange.Core.Internal.Services;

/// <summary>
/// Derive from this class to create a service that handles Oidb packets, enable automatic marshalling and unmarshalling of packets and error handling.
/// </summary>
internal abstract class OidbService<TEventReq, TEventResp, TRequest, TResponse> : BaseService<TEventReq, TEventResp> 
    where TEventReq : ProtocolEvent 
    where TEventResp : ProtocolEvent
    where TRequest : IProtoSerializable<TRequest>
    where TResponse : IProtoSerializable<TResponse>
{
    private protected abstract uint Command { get; }
    
    private protected abstract uint Service { get; }

    private protected virtual uint Reserved { get; } = 0;

    private string Tag => $"OidbSvcTrpcTcp.0x{Command:X}_{Service}";
    
    private protected abstract Task<TRequest> ProcessRequest(TEventReq request, BotContext context);
    
    private protected abstract Task<TEventResp> ProcessResponse(TResponse response, BotContext context);
    
    protected override async ValueTask<TEventResp> Parse(ReadOnlyMemory<byte> input, BotContext context)
    {
        var oidb = ProtoHelper.Deserialize<Oidb>(input.Span);
        if (oidb.Result != 0)
        {
            context.LogWarning(Tag, "Error: {0}, Message: {1}", null, oidb.Result, oidb.Message);
            throw new OperationException((int)oidb.Result, oidb.Message);
        }
        
        return await ProcessResponse(ProtoHelper.Deserialize<TResponse>(oidb.Body.Span), context);
    }

    protected override async ValueTask<ReadOnlyMemory<byte>> Build(TEventReq input, BotContext context)
    {
        var request = await ProcessRequest(input, context);
        var proto = ProtoHelper.Serialize(request);
        return ProtoHelper.Serialize(new Oidb { Command = Command, Service = Service, Body = proto, Reserved = Reserved });
    }
}