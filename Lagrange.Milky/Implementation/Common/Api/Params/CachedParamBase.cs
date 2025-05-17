using System.Text.Json.Serialization;

namespace Lagrange.Milky.Implementation.Common.Api.Params;

public class CachedParam
{
    public bool? NoCache { get; init; }
}