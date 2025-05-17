using Lagrange.Core.Common;

namespace Lagrange.Milky.Core.Configuration;

public class ProtocolConfiguration
{
    public Protocols? Protocol { get; set; }

    public bool GetOptimumServer { get; set; } = true;

    public bool UseIPv6Network { get; set; } = false;

    public string DeviceIdentifier { get; set; } = "LGR-Milky";

    public SignerConfiguration Signer { get; set; } = new();
}