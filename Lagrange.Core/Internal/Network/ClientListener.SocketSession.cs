using System.Net.Sockets;

namespace Lagrange.Core.Internal.Network;

internal abstract partial class ClientListener
{
    protected sealed class SocketSession : IDisposable
    {
        public Socket Socket { get; }

        private CancellationTokenSource? _cts;

        public CancellationToken Token { get; }

        public SocketSession(bool useIPv6 = false)
        {
            Socket = new Socket(useIPv6 ? AddressFamily.InterNetworkV6 : AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _cts = new CancellationTokenSource();
            Token = _cts.Token;
        }

        public void Dispose()
        {
            var cts = Interlocked.Exchange(ref _cts, null);
            if (cts == null) return;
            
            cts.Cancel();
            cts.Dispose();
            Socket.Dispose();
        }
    }
}