using System.Collections.Frozen;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Lagrange.Core.Events;
using Lagrange.Core.Internal.Events;
using Lagrange.Core.Internal.Logic;
using Lagrange.Core.Internal.Packets.Struct;
using Lagrange.Core.Utility.Extension;

namespace Lagrange.Core.Internal.Context;

internal class EventContext : IDisposable
{
    private readonly BotContext _context;
    
    private readonly FrozenDictionary<Type, List<ILogic>> _events;

    private readonly FrozenDictionary<Type, ILogic> _logics;

    [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "All the types are preserved in the csproj by using the TrimmerRootAssembly attribute")]
    [UnconditionalSuppressMessage("Trimming", "IL2062", Justification = "All the types are preserved in the csproj by using the TrimmerRootAssembly attribute")]
    [UnconditionalSuppressMessage("Trimming", "IL2072", Justification = "All the types are preserved in the csproj by using the TrimmerRootAssembly attribute")]
    public EventContext(BotContext context)
    {
        var events = new Dictionary<Type, List<ILogic>>();
        var logics = new Dictionary<Type, ILogic>();
        
        foreach (var type in typeof(ILogic).Assembly.GetTypes())
        {
            if (type.HasImplemented<ILogic>() && Activator.CreateInstance(type, context) is ILogic instance)
            {
                foreach (var @event in type.GetCustomAttributes<EventSubscribeAttribute>())
                {
                    if (!events.TryGetValue(@event.EventType, out var list))
                    {
                        list = [];
                        events.Add(@event.EventType, list);
                    }
                    
                    list.Add(instance);
                }
                
                logics[type] = instance;
            }
        }
        
        _context = context;
        _events = events.ToFrozenDictionary();
        _logics = logics.ToFrozenDictionary();
    }
    
    public async ValueTask<T?> SendEvent<T>(ProtocolEvent @event) where T : ProtocolEvent
    {
        try
        {
            await HandleOutgoingEvent(@event);
            var (frame, attribute) =  await _context.ServiceContext.Resolve(@event);
            if (frame.Sequence == 0)
            {
                // TODO: Log
                return null;
            }
            
            var @return = await _context.PacketContext.SendPacket(frame, attribute);
            var resolved = await _context.ServiceContext.Resolve(@return);
            
            if (resolved is T result)
            {
                await HandleIncomingEvent(result);
                return result;
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }

        return null;
    }
    
    private async ValueTask HandleIncomingEvent(ProtocolEvent @event)
    {
        if (_events.TryGetValue(@event.GetType(), out var logics))
        {
            foreach (var logic in logics)
            {
                try
                {
                    await logic.Incoming(@event);
                }
                catch (Exception e)
                {
                    // TODO: Log
                }
            }
        }
    }

    private async ValueTask HandleOutgoingEvent(ProtocolEvent @event)
    {
        if (_events.TryGetValue(@event.GetType(), out var logics))
        {
            foreach (var logic in logics)
            {
                try
                {
                    await logic.Outgoing(@event);
                }
                catch (Exception e)
                {
                    // TODO: Log
                }
            }
        }
    }
    
    public async ValueTask HandleOutgoingEvent(EventBase @event)
    {
        if (_events.TryGetValue(@event.GetType(), out var logics))
        {
            foreach (var logic in logics)
            {
                try
                {
                    await logic.Outgoing(@event);
                }
                catch (Exception e)
                {
                    // TODO: Log
                }
            }
        }
    }

    public async Task HandleServerPacket(SsoPacket packet)
    {
        var @event = await _context.ServiceContext.Resolve(packet);
        if (@event is not null) await HandleIncomingEvent(@event);
    }
    
    public T GetLogic<T>() where T : ILogic => (T)_logics[typeof(T)];

    public void Dispose()
    {
        foreach (var logic in _logics.Values)
        {
            if (logic is IDisposable disposable) disposable.Dispose();
        }
    }
}