namespace Lagrange.Milky.Core.Configuration;

public class LagrangeConfiguration
{
    public ProtocolConfiguration Protocol { get; set; } = new();

    public LoginConfiguration Login { get; set; } = new();
}