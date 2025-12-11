using ManagedDoom;
using ManagedDoom.Audio;

namespace UnoDoom.Game;

public class UnoSound : ISound, IDisposable
{
    public UnoSound(Config config, GameContent content)
    {
        // TODO: Implement sound system
        // For now, use null sound (no audio)
        Console.WriteLine("Sound system not yet implemented");
    }

    public void SetListener(Mobj listener)
    {
    }

    public void Update()
    {
    }

    public void StartSound(Sfx sfx)
    {
    }

    public void StartSound(Mobj mobj, Sfx sfx, SfxType type)
    {
    }

    public void StartSound(Mobj mobj, Sfx sfx, SfxType type, int volume)
    {
    }

    public void StopSound(Mobj mobj)
    {
    }

    public void Reset()
    {
    }

    public void Pause()
    {
    }

    public void Resume()
    {
    }

    public void Dispose()
    {
    }

    public int MaxVolume => 15;

    public int Volume { get; set; } = 8;
}
