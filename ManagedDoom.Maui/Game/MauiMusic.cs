using ManagedDoom.Audio;
using Plugin.Maui.Audio;
using System.Runtime.ExceptionServices;

namespace ManagedDoom.Maui.Game
{
    public sealed class MauiMusic : NullMusic, IMusic, IDisposable
    {
        public MauiMusic(Config config, GameContent content, IAudioManager audioManager, string sfPath)
        {
            //todo
        }
    }
}
