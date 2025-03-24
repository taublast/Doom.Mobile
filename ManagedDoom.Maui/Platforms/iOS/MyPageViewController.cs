using Microsoft.Maui.Platform;

namespace ManagedDoom.Maui;

public class MyPageViewController : PageViewController
{
    public MyPageViewController(IView page, IMauiContext mauiContext) : base(page, mauiContext)
    {
            
    }

    public override bool PrefersHomeIndicatorAutoHidden
    {
        get
        {
            return true;
        }
    }
}