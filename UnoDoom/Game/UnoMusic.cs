using ManagedDoom;
using ManagedDoom.Audio;

namespace UnoDoom.Game;

public class UnoMusic : IMusic, IDisposable
{
    public UnoMusic(Config config, GameContent content)
    {
        // TODO: Implement music system
        Console.WriteLine("Music system not yet implemented");
    }

    public void StartMusic(Bgm bgm, bool loop)
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
