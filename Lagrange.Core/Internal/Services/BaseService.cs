using Lagrange.Core.Internal.Events;

namespace Lagrange.Core.Internal.Services;

internal abstract class BaseService<T> : IService where T : ProtocolEvent
{
    protected virtual Task<ProtocolEvent?> Parse(ReadOnlyMemory<byte> input, BotContext context) => Task.FromResult<ProtocolEvent?>(null);
    
    protected virtual Task<ReadOnlyMemory<byte>> Build(T input, BotContext context) => Task.FromResult(ReadOnlyMemory<byte>.Empty);
    
    Task<ProtocolEvent?> IService.Parse(ReadOnlyMemory<byte> input, BotContext context) => Parse(input, context);
    
    Task<ReadOnlyMemory<byte>> IService.Build(ProtocolEvent input, BotContext context) => Build((T)input, context);
}