using Plugin.Maui.Audio;

namespace ManagedDoom.Maui.Game
{
    public partial class ConfigUtilities : IConfigUtilities
    {
        public static Config GetConfig()
        {
            var config = new Config(PlatformHelpers.ConfigUtilities.GetConfigPath());

            if (!config.IsRestoredFromFile)
            {
                config.video_screenwidth = 640;
                config.video_screenheight = 400;
            }

            return config;
        }


        static object _lockFiles = new();

        private static readonly string[] iwadNames = new string[]
        {
            "doom2.wad",
            "plutonia.wad",
            "tnt.wad",
            "doom.wad",
            "doom1.wad",
            "freedoom2.wad",
            "freedoom1.wad"
        };

        /// <summary>
        /// Get file from bundle resources and extract it to appdata folder on device
        /// </summary>
        /// <param name="name"></param>
        public static bool InsureResourceExtracted(string name)
        {
            lock (_lockFiles)
            {
                var sfPath = Path.Combine(PlatformHelpers.ConfigUtilities.GetExeDirectory(), name);
                if (!File.Exists(sfPath))
                {
                    var filenameDb = Path.Combine(FileSystem.AppDataDirectory, name);
                    if (!File.Exists(filenameDb))
                    {
                        using var stream = FileSystem.OpenAppPackageFileAsync(name).GetAwaiter().GetResult();
                        if (stream == null)
                            return false;
                        using (var memoryStream = new MemoryStream())
                        {
                            stream.CopyTo(memoryStream);
                            File.WriteAllBytes(filenameDb, memoryStream.ToArray());
                        }

                    }
                }
                return true;
            }
        }

        public string GetExeDirectory()
        {
            return FileSystem.AppDataDirectory;
        }

        public string GetConfigPath()
        {
            return Path.Combine(GetExeDirectory(), "managed-doom.cfg");
        }

        public string GetDefaultIwadPath()
        {

            var exeDirectory = GetExeDirectory();
            foreach (var name in iwadNames)
            {
                var path = Path.Combine(exeDirectory, name);
                if (InsureResourceExtracted(name))
                    return path;
            }

            throw new Exception("No IWAD was found!");
        }

        public static MauiMusic GetMusicInstance(Config config, GameContent content, IAudioManager audioManager)
        {
            InsureResourceExtracted(config.audio_soundfont);
            var sfPath = Path.Combine(PlatformHelpers.ConfigUtilities.GetExeDirectory(), config.audio_soundfont);
            if (File.Exists(sfPath))
            {
                return new MauiMusic(config, content, audioManager, sfPath);
            }
            else
            {
                Console.WriteLine("SoundFont '" + config.audio_soundfont + "' was not found!");
                return null;
            }
        }


        public bool IsIwad(string path)
        {
            var name = Path.GetFileName(path).ToUpper();
            return iwadNames.Contains(name);
        }

        public string[] GetWadPaths(CommandLineArgs args)
        {
            var wadPaths = new List<string>();

            if (args.iwad.Present)
            {
                wadPaths.Add(args.iwad.Value);
            }
            else
            {
                wadPaths.Add(PlatformHelpers.ConfigUtilities.GetDefaultIwadPath());
            }

            if (args.file.Present)
            {
                foreach (var path in args.file.Value)
                {
                    wadPaths.Add(path);
                }
            }

            return wadPaths.ToArray();
        }
    }
}
