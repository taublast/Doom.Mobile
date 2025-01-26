using System.Diagnostics;

namespace ManagedDoom.Maui.Game;

public struct EventTimestamp
{
    public EventTimestamp(long frame)
    {
        this.Timestamp = Stopwatch.GetTimestamp() / (Stopwatch.Frequency / 1000); // ms;
        this.Frame = frame;
    }

    public static EventTimestamp Empty
    {
        get
        {
            return new EventTimestamp(0);
        }
    }

    public bool IsEmpty => Frame == 0;
    public bool HasValue => Frame > 0;

    public bool Expired(EventTimestamp now, int msDelay)
    {
        if (this.Frame > 0 && this.Frame != now.Frame && now.Timestamp - this.Timestamp > msDelay)
        {
            return true;
        }
        return false;
    }

    public long Frame { get; set; }
    public long Timestamp { get; set; }
}