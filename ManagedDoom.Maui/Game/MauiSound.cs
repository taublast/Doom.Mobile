using ManagedDoom.Audio;
using Plugin.Maui.Audio;
using System;
using System.Numerics;
using System.Runtime.ExceptionServices;
using System.Threading.Channels;

namespace ManagedDoom.Maui.Game
{
    public sealed class MauiSound : ISound, IDisposable
    {
        private static readonly int channelCount = 8;

        private readonly Config config;
        private readonly AudioMixer soundMixer;

        private IAudioSource[] buffers;
        private float[] amplitudes;

        private DoomRandom random;

        private IAudioPlayer uiChannel;
        private Sfx uiReserved;

        private Mobj listener;

        private float masterVolumeDecay;

        private DateTime lastUpdate;

        // Array to hold ChannelInfo for each channel managed by AudioMixer
        private ChannelInfo[] channelInfos;
        private readonly bool _desktop;

        public MauiSound(Config config, GameContent content, IAudioManager audioManager)
        {
            try
            {
                Console.Write("Initialize sound: ");

                _desktop = !MauiProgram.IsMobile;

                this.config = config;

                config.audio_soundvolume = Math.Clamp(config.audio_soundvolume, 0, MaxVolume);
                masterVolumeDecay = (float)config.audio_soundvolume / MaxVolume;

                buffers = new IAudioSource[DoomInfo.SfxNames.Length];
                amplitudes = new float[DoomInfo.SfxNames.Length];
                channelInfos = new ChannelInfo[channelCount];

                // Optional random pitch
                if (config.audio_randompitch)
                {
                    random = new DoomRandom();
                }

                // Load WAD lumps (PCM data) into memory
                for (var i = 0; i < DoomInfo.SfxNames.Length; i++)
                {
                    string wadName = "DS" + DoomInfo.SfxNames[i].ToString().ToUpper();
                    int lump = content.Wad.GetLumpNumber(wadName);
                    if (lump == -1)
                    {
                        continue;
                    }

                    int sampleRate, sampleCount;
                    var samples = GetSamples(content.Wad, wadName, out sampleRate, out sampleCount);
                    if (!samples.IsEmpty)
                    {
                        buffers[i] = new RawAudioSource(samples, sampleRate, 1);

                        // Compute amplitude for priority calculations
                        amplitudes[i] = GetAmplitude(samples, sampleRate, sampleCount);
                    }
                }

                soundMixer = new AudioMixer(audioManager, channelCount);

                if (!_desktop)
                    soundMixer.MapBalance(-1); //rotated right 90 degrees

                // Initialize ChannelInfo for each channel
                for (int i = 0; i < channelCount; i++)
                {
                    channelInfos[i] = new ChannelInfo();
                }

                // Initialize UI channel separately
                uiChannel = audioManager.CreatePlayer();
                uiReserved = Sfx.NONE;

                lastUpdate = DateTime.MinValue;

                Console.WriteLine("OK");
            }
            catch (Exception e)
            {
                Console.WriteLine("Failed");
                Dispose();
                ExceptionDispatchInfo.Throw(e);
            }
        }

        private static Span<byte> GetSamples(Wad wad, string name, out int sampleRate, out int sampleCount)
        {
            var data = wad.ReadLump(name);

            if (data.Length < 8)
            {
                sampleRate = -1;
                sampleCount = -1;
                return Span<byte>.Empty;
            }

            sampleRate = BitConverter.ToUInt16(data, 2);
            sampleCount = BitConverter.ToInt32(data, 4);

            var offset = 8;
            if (ContainsDmxPadding(data))
            {
                offset += 16;
                sampleCount -= 32;
            }

            if (sampleCount > 0 && (offset + sampleCount) <= data.Length)
            {
                return data.AsSpan(offset, sampleCount);
            }
            else
            {
                return Span<byte>.Empty;
            }
        }

        // Check if the data contains pad bytes.
        // If the first and last 16 samples are the same,
        // the data should contain pad bytes.
        // https://doomwiki.org/wiki/Sound
        private static bool ContainsDmxPadding(byte[] data)
        {
            var sampleCount = BitConverter.ToInt32(data, 4);
            if (sampleCount < 32)
            {
                return false;
            }
            else
            {
                var first = data[8];
                for (var i = 1; i < 16; i++)
                {
                    if (data[8 + i] != first)
                    {
                        return false;
                    }
                }

                var last = data[8 + sampleCount - 1];
                for (var i = 1; i < 16; i++)
                {
                    if (data[8 + sampleCount - i - 1] != last)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private static float GetAmplitude(Span<byte> samples, int sampleRate, int sampleCount)
        {
            var max = 0;
            if (sampleCount > 0)
            {
                var count = Math.Min(sampleRate / 5, sampleCount);
                for (var t = 0; t < count; t++)
                {
                    var a = samples[t] - 128;
                    if (a < 0)
                    {
                        a = -a;
                    }
                    if (a > max)
                    {
                        max = a;
                    }
                }
            }
            return (float)max / 128;
        }

        public void SetListener(Mobj listener)
        {
            this.listener = listener;
        }

        private float GetPitch(SfxType type, Sfx sfx)
        {
            if (random != null)
            {
                if (sfx == Sfx.ITEMUP || sfx == Sfx.TINK || sfx == Sfx.RADIO)
                {
                    return 1.0F;
                }
                else if (type == SfxType.Voice)
                {
                    return 1.0F + 0.075F * (random.Next() - 128) / 128;
                }
                else
                {
                    return 1.0F + 0.025F * (random.Next() - 128) / 128;
                }
            }
            else
            {
                return 1.0F;
            }
        }

        public void Update()
        {
            var now = DateTime.Now;
            if ((now - lastUpdate).TotalSeconds < 0.01)
            {
                // Don't update so frequently (for timedemo).
                return;
            }

            for (var i = 0; i < channelInfos.Length; i++)
            {
                var info = channelInfos[i];
                var player = soundMixer.Channels[i];

                if (info.Playing != Sfx.NONE)
                {
                    if (player.IsPlaying)
                    {
                        if (info.Type == SfxType.Diffuse)
                        {
                            info.Priority *= soundMixer.SlowDecay;
                        }
                        else
                        {
                            info.Priority *= soundMixer.FastDecay;
                        }
                        SetParam(player, info); // SetParam utilizes CalculateSpatial
                    }
                    else
                    {
                        // Sound has finished playing
                        info.Playing = Sfx.NONE;
                        if (info.Reserved == Sfx.NONE)
                        {
                            info.Source = null;
                        }
                    }
                }

                if (info.Reserved != Sfx.NONE)
                {
                    if (info.Playing != Sfx.NONE)
                    {
                        soundMixer.Stop(i);
                    }

                    // Set the audio source for the channel
                    soundMixer.SetSource(i, buffers[(int)info.Reserved]);
                    var channel = soundMixer.Channels[i];
                    SetParam(channel, info);
                    channel.Speed = GetPitch(info.Type, info.Reserved);
                    soundMixer.Play(i, buffers[(int)info.Reserved], loop: false);
                    info.Playing = info.Reserved;
                    info.Reserved = Sfx.NONE;
                }
            }

            //UI channel
            if (uiReserved != Sfx.NONE)
            {
                if (uiChannel.IsPlaying)
                {
                    uiChannel.Stop();
                }

                uiChannel.Volume = masterVolumeDecay;
                uiChannel.SetSource(buffers[(int)uiReserved].GetAudioStream());
                soundMixer.Play(channelCount - 1, buffers[(int)uiReserved], loop: false);
                uiReserved = Sfx.NONE;
            }

            lastUpdate = now;
        }

        /// <summary>
        /// Adjusts volume and stereo panning based on the channel information.
        /// Utilizes the Balance property for panning based on the source's position relative to the listener.
        /// </summary>
        /// <param name="sound">The audio player to adjust.</param>
        /// <param name="info">The channel information.</param>
        private void SetParam(IAudioPlayer sound, ChannelInfo info)
        {
            if (info.Type == SfxType.Diffuse)
            {
                sound.Balance = 0;
                sound.Volume = 0.01F * masterVolumeDecay * info.Volume);
            }
            else
            {
                // Positional sound
                Fixed sourceX;
                Fixed sourceY;

                if (info.Source == null)
                {
                    sourceX = info.LastX;
                    sourceY = info.LastY;
                }
                else
                {
                    sourceX = info.Source.X;
                    sourceY = info.Source.Y;
                }

                float x = (sourceX - listener.X).ToFloat();
                float y = (sourceY - listener.Y).ToFloat();

                // Compute distance for attenuation
                if (Math.Abs(x) < 16 && Math.Abs(y) < 16)
                {
                    sound.Balance = 0;
                    sound.Volume = 0.01F * masterVolumeDecay * info.Volume;
                }
                else
                {
                    float dist = MathF.Sqrt(x * x + y * y);
                    var angle = (float)(MathF.Atan2(y, x) - listener.Angle.ToRadian());

                    Vector3 direction = new Vector3(-MathF.Sin(angle), 0, -MathF.Cos(angle));
                    float baseBalance = MathF.Sin(angle);
                    float attenuation = soundMixer.GetDistanceDecay(dist);
                    float baseVolume = 0.01f * masterVolumeDecay * attenuation * info.Volume;
                    var spatial = soundMixer.PositionInSpace(direction, baseVolume, baseBalance);

                    sound.Volume = spatial.Volume;
                    sound.Balance = spatial.Balance;
                }
            }
        }

        /// <summary>
        /// Retrieves the channel index for a given audio player.
        /// </summary>
        /// <param name="player">The audio player.</param>
        /// <returns>The channel index.</returns>
        private int GetChannelIndex(IAudioPlayer player)
        {
            for (int i = 0; i < soundMixer.Channels.Count; i++)
            {
                if (soundMixer.Channels[i] == player)
                {
                    return i;
                }
            }
            return -1; // Not found
        }

        public void StartSound(Sfx sfx)
        {
            if (buffers[(int)sfx] == null)
            {
                return;
            }
            // Reserve the UI channel (assuming UI uses the last channel)
            uiReserved = sfx;
        }

        public void StartSound(Mobj mobj, Sfx sfx, SfxType type)
        {
            StartSound(mobj, sfx, type, 100);
        }

        public void StartSound(Mobj mobj, Sfx sfx, SfxType type, int volume)
        {
            if (buffers[(int)sfx] == null)
            {
                return;
            }

            float x = (mobj.X - listener.X).ToFloat();
            float y = (mobj.Y - listener.Y).ToFloat();
            float dist = MathF.Sqrt(x * x + y * y);

            float priority;
            if (type == SfxType.Diffuse)
            {
                priority = volume;
            }
            else
            {
                priority = amplitudes[(int)sfx] * soundMixer.GetDistanceDecay(dist) * volume;
            }

            // Check if the same Mobj+type is already playing on a channel
            for (int i = 0; i < channelInfos.Length; i++)
            {
                var info = channelInfos[i];
                if (info.Source == mobj && info.Type == type)
                {
                    // Just replace it with the new Sfx
                    info.Reserved = sfx;
                    info.Priority = priority;
                    info.Volume = volume;
                    return;
                }
            }

            // Else, find a free channel
            for (int i = 0; i < channelInfos.Length; i++)
            {
                var info = channelInfos[i];
                if (info.Reserved == Sfx.NONE && info.Playing == Sfx.NONE)
                {
                    info.Reserved = sfx;
                    info.Priority = priority;
                    info.Source = mobj;
                    info.Type = type;
                    info.Volume = volume;
                    return;
                }
            }

            // If none free, kill the lowest priority
            float minPriority = float.MaxValue;
            int minChannel = -1;
            for (int i = 0; i < channelInfos.Length; i++)
            {
                var info = channelInfos[i];
                if (info.Priority < minPriority)
                {
                    minPriority = info.Priority;
                    minChannel = i;
                }
            }

            if (minChannel >= 0 && priority >= minPriority)
            {
                var info = channelInfos[minChannel];
                info.Reserved = sfx;
                info.Priority = priority;
                info.Source = mobj;
                info.Type = type;
                info.Volume = volume;
            }
        }

        public void StopSound(Mobj mobj)
        {
            for (int i = 0; i < channelInfos.Length; i++)
            {
                var info = channelInfos[i];
                if (info.Source == mobj)
                {
                    // Mark channel so that next update it will fade out or just stop
                    info.LastX = info.Source.X;
                    info.LastY = info.Source.Y;
                    info.Source = null;
                    info.Volume /= 5;
                    // This replicates the “soft stop” in the original code
                }
            }
        }

        public void Reset()
        {
            if (random != null)
            {
                random.Clear();
            }

            // Stop all channels & clear their info
            for (int i = 0; i < channelInfos.Length; i++)
            {
                soundMixer.Stop(i);
                channelInfos[i].Clear();
            }

            listener = null;
        }

        public void Pause()
        {
            // Pause all channels via AudioMixer
            soundMixer.PauseAll();

            // Pause UI channel
            if (uiChannel.IsPlaying)
            {
                uiChannel.Pause();
            }
        }

        public void Resume()
        {
            // Resume all channels via AudioMixer
            soundMixer.ResumeAll();

            // Resume UI channel
            if (!uiChannel.IsPlaying)
            {
                uiChannel.Play();
            }
        }

        /// <summary>
        /// Disposes all managed resources.
        /// </summary>
        public void Dispose()
        {
            Console.WriteLine("Shutdown sound.");

            soundMixer.Dispose();

            // Dispose UI channel
            if (uiChannel != null)
            {
                uiChannel.Stop();
                uiChannel.Dispose();
                uiChannel = null;
            }

            // Clear the buffers
            if (buffers != null)
            {
                // Let GC handle raw byte[] arrays
                buffers = null;
            }
        }

        public int MaxVolume => 15;

        public int Volume
        {
            get => config.audio_soundvolume;
            set
            {
                config.audio_soundvolume = value;
                masterVolumeDecay = (float)config.audio_soundvolume / MaxVolume;
            }
        }

        /// <summary>
        /// Represents the information for each audio channel.
        /// </summary>
        private class ChannelInfo
        {
            public Sfx Reserved { get; set; }
            public Sfx Playing { get; set; }
            public float Priority { get; set; }

            public Mobj Source { get; set; }
            public SfxType Type { get; set; }
            public int Volume { get; set; }
            public Fixed LastX { get; set; }
            public Fixed LastY { get; set; }

            public void Clear()
            {
                Reserved = Sfx.NONE;
                Playing = Sfx.NONE;
                Priority = 0f;
                Source = null;
                Type = SfxType.None;
                Volume = 0;
                LastX = Fixed.Zero;
                LastY = Fixed.Zero;
            }
        }
    }
}
