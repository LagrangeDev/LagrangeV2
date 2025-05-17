using System.Collections.Frozen;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Lagrange.Core.Common;
using Lagrange.Core.Exceptions;
using Lagrange.Core.Internal.Events;
using Lagrange.Core.Internal.Packets.Struct;
using Lagrange.Core.Internal.Services;
using Lagrange.Core.Utility.Extension;

namespace Lagrange.Core.Internal.Context;

internal class ServiceContext
{
    private const string Tag = nameof(ServiceContext);

    private int _sequence = Random.Shared.Next(5000000, 9900000);

    private readonly FrozenDictionary<string, IService> _services;
    private readonly FrozenDictionary<Type, (ServiceAttribute Attribute, IService Instance)> _servicesEventType;

    private readonly BotContext _context;

    [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "All the types are preserved in the csproj by using the TrimmerRootAssembly attribute")]
    [UnconditionalSuppressMessage("Trimming", "IL2062", Justification = "All the types are preserved in the csproj by using the TrimmerRootAssembly attribute")]
    [UnconditionalSuppressMessage("Trimming", "IL2072", Justification = "All the types are preserved in the csproj by using the TrimmerRootAssembly attribute")]
    public ServiceContext(BotContext context)
    {
        _context = context;

        var services = new Dictionary<string, IService>();
        var servicesEventType = new Dictionary<Type, (ServiceAttribute, IService)>();

        foreach (var type in typeof(IService).Assembly.GetTypes())
        {
            foreach (var attribute in type.GetCustomAttributes<EventSubscribeAttribute>())
            {
                if ((~attribute.Protocol & context.Config.Protocol) != Protocols.None) continue; // skip if not supported

                if (type.GetCustomAttribute<ServiceAttribute>() is { } attr && type.HasImplemented<IService>())
                {
                    if (!services.TryGetValue(attr.Command, out var service))
                    {
                        service = (IService?)Activator.CreateInstance(type) ?? throw new InvalidOperationException("Failed to create service instance");
                        services[attr.Command] = service;
                    }

                    servicesEventType[attribute.EventType] = !servicesEventType.ContainsKey(attribute.EventType)
                        ? (attr, service)
                        : throw new InvalidOperationException($"Multiple services for event type: {attribute.EventType}");
                }
            }
        }

        _services = services.ToFrozenDictionary();
        _servicesEventType = servicesEventType.ToFrozenDictionary();
    }

    public ValueTask<ProtocolEvent> Resolve(SsoPacket ssoPacket)
    {
        if (!_services.TryGetValue(ssoPacket.Command, out var service))
        {
            throw new ServiceNotFoundException(ssoPacket.Command);
        }

        if (ssoPacket.Command != "Heartbeat.Alive")
        {
            _context.LogDebug(Tag, "Incoming SSOFrame: {0}", ssoPacket.Command);
        }
        return service.Parse(ssoPacket.Data, _context);
    }

    public async ValueTask<(SsoPacket, ServiceAttribute)> Resolve(ProtocolEvent @event)
    {
        if (!_servicesEventType.TryGetValue(@event.GetType(), out var handler)) return default;

        var (attr, service) = handler;
        if (handler.Attribute.Command != "Heartbeat.Alive")
        {
            _context.LogDebug(Tag, "Outgoing SSOFrame: {0}", handler.Attribute.Command);
        }

        return (new SsoPacket(attr.Command, await service.Build(@event, _context), GetNewSequence()), attr);
    }

    private int GetNewSequence()
    {
        Interlocked.CompareExchange(ref _sequence, 5000000, 9900000);
        return Interlocked.Increment(ref _sequence);
    }
}