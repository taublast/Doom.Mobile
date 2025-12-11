using System.Net.Http;

namespace UnoDoom.Game;

public static class AssetLoader
{
    private static readonly HttpClient httpClient = new HttpClient();

    /// <summary>
    /// Downloads an asset file and saves it to a temporary location
    /// </summary>
    public static async Task<string> PrepareAssetAsync(string assetPath)
    {
#if __WASM__
        // For WebAssembly, download the asset via HTTP and save to a temporary location
        var response = await httpClient.GetAsync(assetPath);
        response.EnsureSuccessStatusCode();
        
        var assetData = await response.Content.ReadAsByteArrayAsync();
        var tempPath = Path.GetTempFileName() + Path.GetExtension(assetPath);
        await File.WriteAllBytesAsync(tempPath, assetData);
        return tempPath;
#else
        // For other platforms, the path is already valid
        return assetPath;
#endif
    }

    /// <summary>
    /// Prepares multiple asset files asynchronously
    /// </summary>
    public static async Task<string[]> PrepareAssetsAsync(params string[] assetPaths)
    {
        var tasks = assetPaths.Select(PrepareAssetAsync);
        return await Task.WhenAll(tasks);
    }
}