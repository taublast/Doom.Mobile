using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using ContentView = Microsoft.Maui.Platform.ContentView;

namespace ManagedDoom.Maui;

public class MyPageHandler : PageHandler
{
    protected override ContentView CreatePlatformView()
    {
        _ = VirtualView ?? throw new InvalidOperationException($"{nameof(VirtualView)} must be set to create a LayoutView");
        _ = MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} cannot be null");

        if (ViewController == null)
        {
            ViewController = new MyPageViewController(VirtualView, MauiContext);
        }

        if (ViewController is PageViewController pc && pc.CurrentPlatformView is ContentView pv)
            return pv;

        if (ViewController.View is ContentView cv)
            return cv;

        throw new InvalidOperationException($"PageViewController.View must be a {nameof(ContentView)}");

    }
}