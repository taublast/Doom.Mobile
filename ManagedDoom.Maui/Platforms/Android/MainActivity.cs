using Android.App;
using Android.Content.PM;
using Android.OS;

namespace ManagedDoom.Maui
{
    [Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true,
        LaunchMode = LaunchMode.SingleTask,
        ScreenOrientation = ScreenOrientation.Landscape,
        ConfigurationChanges = ConfigChanges.ScreenSize
                               | ConfigChanges.Orientation
                               | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
    public class MainActivity : MauiAppCompatActivity
    {
    }
}
