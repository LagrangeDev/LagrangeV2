using Lagrange.Core.Internal.Events;

namespace Lagrange.Core.Internal.Services;

internal abstract class BaseService<T> : IService where T : ProtocolEvent
{
    protected virtual ValueTask<ProtocolEvent?> Parse(ReadOnlyMemory<byte> input, BotContext context) => ValueTask.FromResult<ProtocolEvent?>(null);
    
    protected virtual ValueTask<ReadOnlyMemory<byte>> Build(T input, BotContext context) => ValueTask.FromResult(ReadOnlyMemory<byte>.Empty);
    
    ValueTask<ProtocolEvent?> IService.Parse(ReadOnlyMemory<byte> input, BotContext context) => Parse(input, context);
    
    ValueTask<ReadOnlyMemory<byte>> IService.Build(ProtocolEvent input, BotContext context) => Build((T)input, context);
}