namespace Lagrange.Core.Events;

public abstract class EventBase : System.EventArgs
{
    /// <summary>
    /// Local receipt time, not server arrival time.
    /// </summary>
    public DateTime EventTime { get; }
    
    internal EventBase() => EventTime = DateTime.UtcNow;

    public abstract string ToEventMessage();
    
    public override string ToString() => $"[{EventTime:yyyy-MM-dd HH:mm:ss}] {ToEventMessage()}";
}