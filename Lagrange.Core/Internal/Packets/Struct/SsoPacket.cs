using System.Threading.Tasks.Sources;

namespace Lagrange.Core.Internal.Packets.Struct;

internal class SsoPacket(string command, int sequence, int retCode, string extra)
{
    public ReadOnlyMemory<byte> Data { get; }

    public string Command { get; } = command;

    public string Extra { get; } = extra;

    public int RetCode { get; } = retCode;

    public int Sequence { get; } = sequence;

    public SsoPacket(string command, ReadOnlyMemory<byte> data, int sequence) : this(command, sequence, 0, string.Empty) => Data = data;
}

internal class SsoPacketValueTaskSource : IValueTaskSource<SsoPacket>
{
    private ManualResetValueTaskSourceCore<SsoPacket> _core = new()
    {
        RunContinuationsAsynchronously = true,
    };

    public SsoPacket GetResult(short token) => _core.GetResult(token);

    public ValueTaskSourceStatus GetStatus(short token) => _core.GetStatus(token);

    public void OnCompleted(Action<object?> continuation, object? state, short token, ValueTaskSourceOnCompletedFlags flags) => _core.OnCompleted(continuation, state, token, flags);

    public void SetResult(SsoPacket result) => _core.SetResult(result);

    public void SetException(Exception exception) => _core.SetException(exception);
}