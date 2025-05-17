using System.Collections.Frozen;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Lagrange.Core.Events;
using Lagrange.Core.Exceptions;
using Lagrange.Core.Internal.Events;
using Lagrange.Core.Internal.Logic;
using Lagrange.Core.Internal.Packets.Struct;
using Lagrange.Core.Utility.Extension;

namespace Lagrange.Core.Internal.Context;

internal class EventContext : IDisposable
{
    private const string Tag = nameof(EventContext);
    
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
    
    public async ValueTask<T> SendEvent<T>(ProtocolEvent @event) where T : ProtocolEvent
    {
        try
        {
            await HandleOutgoingEvent(@event);
            var (frame, attribute) =  await _context.ServiceContext.Resolve(@event);
            if (frame.Sequence == 0) throw new LagrangeException("The sequence number is 0 for the SSOFrame");
            
            var @return = await _context.PacketContext.SendPacket(frame, attribute);
            var resolved = await _context.ServiceContext.Resolve(@return);
            
            if (resolved is T result)
            {
                await HandleIncomingEvent(result);
                return result;
            }

            throw new LagrangeException($"The event type is not the same as the expected type. Expected: {typeof(T)}, Actual: {resolved.GetType()}");
        }
        catch (Exception e) when (e is not LagrangeException)
        {
            throw new LagrangeException("An error occurred while sending the event", e);
        }
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
                catch (Exception e) when (e is not LagrangeException)
                {
                    throw new LagrangeException("An error occurred while processing the incoming event", e);
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
                catch (Exception e) when (e is not LagrangeException)
                {
                    throw new LagrangeException("An error occurred while processing the outgoing event", e);
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
                catch (Exception e) when (e is not LagrangeException)
                {
                    throw new LagrangeException("An error occurred while processing the outgoing event", e);
                }
            }
        }
    }

    public async Task HandleServerPacket(SsoPacket packet)
    {
        try
        {
            var @event = await _context.ServiceContext.Resolve(packet);
            await HandleIncomingEvent(@event);
        }
        catch (ServiceNotFoundException e)
        {
            _context.LogWarning(Tag, "Service not found for command: {0}", e.Command);
        }
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