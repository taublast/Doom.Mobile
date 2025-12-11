using ManagedDoom;

namespace UnoDoom.Game;

public class ConfigUtilities : IConfigUtilities
{
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

#if __WASM__
    private static string _localWadPath;

    /// <summary>
    /// Prepares assets for WebAssembly by downloading them via HTTP
    /// </summary>
    public static async Task PrepareAssetsAsync()
    {
        var localFolder = await ApplicationData.Current.LocalFolder.CreateFolderAsync("Wads");
        foreach (var name in iwadNames)
        {
            var assetPath = $"Assets/{name}";
            try
            {
                var fileFromAssets = await StorageFile.GetFileFromApplicationUriAsync(new Uri($"ms-appx:///{assetPath}"));
                if (fileFromAssets != null)
                {
                    // Copy to local folder
                    var copiedFile = await fileFromAssets.CopyAsync(localFolder, name, NameCollisionOption.ReplaceExisting);
                    Console.WriteLine($"Copied IWAD {name} to local folder.");
                    _localWadPath = copiedFile.Path;
                }
            }
            catch
            {
                // Ignore missing assets
            }
        }
    }
#endif

    public Config GetConfig()
    {
        var configPath = GetConfigPath();
        var config = new Config(configPath);

        if (!config.IsRestoredFromFile)
        {
            config.video_screenwidth = 640;
            config.video_screenheight = 400;
        }

        return config;
    }

    public string GetExeDirectory()
    {
#if WINDOWS || __MACCATALYST__ || __MACOS__
        return Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
#else
        return Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
#endif
    }

    public string GetConfigPath()
    {
        return Path.Combine(GetExeDirectory(), "managed-doom.cfg");
    }

    public string GetDefaultIwadPath()
    {
#if __WASM__
        return _localWadPath;

        throw new Exception("No IWAD was found! Make sure to call PrepareAssetsAsync() first.");
#else
        var exeDirectory = GetExeDirectory();
        foreach (var name in iwadNames)
        {
            var path = Path.Combine(exeDirectory, name);
            if (File.Exists(path))
                return path;
        }

        // Check in Assets folder
        var assetsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets");
        foreach (var name in iwadNames)
        {
            var path = Path.Combine(assetsPath, name);
            if (File.Exists(path))
                return path;
        }

        throw new Exception("No IWAD was found!");
#endif
    }

    public bool IsIwad(string path)
    {
        var name = Path.GetFileName(path).ToLower();
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
            wadPaths.Add(GetDefaultIwadPath());
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

    private string GetConfigDirectory()
    {
#if WINDOWS || __MACCATALYST__ || __MACOS__
        return Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
#else
        return Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
#endif
    }
}
