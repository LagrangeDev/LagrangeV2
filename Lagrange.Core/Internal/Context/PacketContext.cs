using System.Collections.Concurrent;
using Lagrange.Core.Common;
using Lagrange.Core.Internal.Packets.Struct;
using Lagrange.Core.Internal.Services;

namespace Lagrange.Core.Internal.Context;

internal class PacketContext(BotContext context)
{
    private readonly ConcurrentDictionary<int, SsoPacketValueTaskSource> _pendingTasks = new();
    
    private readonly BotKeystore _keystore = context.Keystore;
    private readonly SsoPacker _ssoPacker = new(context);
    private readonly ServicePacker _servicePacker = new(context);
    private readonly IBotSignProvider _signProvider = context.Config.SignProvider ?? new DefaultBotSignProvider(context.Config.Protocol, context.AppInfo);

    public ValueTask<SsoPacket> SendPacket(SsoPacket packet, ServiceOptions options)
    {
        var tcs = new SsoPacketValueTaskSource();
        _pendingTasks.TryAdd(packet.Sequence, tcs);

        Task.Run(async () => // Schedule the task to the ThreadPool
        {
            ReadOnlyMemory<byte> frame;
            
            switch (options.RequestType)
            {
                case RequestType.D2Auth:
                {
                    if (IBotSignProvider.IsWhiteListCommand(packet.Command))
                    {
                        var secInfo = await _signProvider.GetSecSign(_keystore.Uin, packet.Command, packet.Sequence, packet.Data);
                        var sso = _ssoPacker.BuildProtocol12(ref packet, secInfo);
                        frame = _servicePacker.BuildProtocol12(ref sso, options);
                    }
                    else
                    {
                        var sso = _ssoPacker.BuildProtocol12(ref packet, null);
                        frame = _servicePacker.BuildProtocol12(ref sso, options);
                    }

                    break;
                }
                case RequestType.Simple:
                {
                    var sso = _ssoPacker.BuildProtocol13(ref packet);
                    frame = _servicePacker.BuildProtocol13(ref packet, ref sso, options);
                    break;
                }
                default:
                {
                    throw new InvalidOperationException($"Unknown RequestType: {options.RequestType}");
                }
            }
            
            await context.SocketContext.Send(frame);
        });
        
        return new ValueTask<SsoPacket>(tcs, 0);
    }
    
    public void ReceivePacket(ReadOnlySpan<byte> buffer)
    {
        
    }

    public void DispatchPacket(ref SsoPacket sso)
    {
        if (_pendingTasks.TryRemove(sso.Sequence, out var tcs))
        {
            if (sso is { RetCode: not 0, Extra: var extra })
            {
                string msg = $"Packet '{sso.Command}' returns {sso.RetCode} with seq: {sso.Sequence}, extra: {extra}";
                tcs.SetException(new InvalidOperationException(msg));
            }
            else
            {
                tcs.SetResult(sso);
            }
        }
        else
        {
            // TODO: Server Allocated Packet
        }
    }
}