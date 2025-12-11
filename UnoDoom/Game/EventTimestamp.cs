namespace UnoDoom.Game;

public struct EventTimestamp
{
    public static readonly EventTimestamp Empty = new EventTimestamp(-1);

    public int Frame { get; set; }

    public EventTimestamp(int frame)
    {
        Frame = frame;
    }

    public bool IsEmpty => Frame < 0;
}
