using DrawnUi.Maui.Controls;
using DrawnUi.Maui.Draw;
using DrawnUi.Maui.Views;

namespace ManagedDoom.Maui;

/// <summary>
/// This is to support code-behind c# HotReload with ease
/// </summary>
public class BaseCodePage : MyPage, IDisposable
{

    public BaseCodePage()
    {
        this.Loaded += (s, a) =>
        {
            Build();
        };

#if DEBUG
        Super.HotReload += ReloadUi;
#endif
    }

    public int CountReloads { get; protected set; }

    /// <summary>
    /// Reload code-behind constructed page
    /// </summary>
    /// <param name="obj"></param>
    protected virtual void ReloadUi(Type[] obj)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            CountReloads++;
            Build();
        });
    }

    /// <summary>
    /// Build code-behind constructed page
    /// </summary>
    public virtual void Build()
    {

    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public bool IsDisposed { get; protected set; }

    protected virtual void Dispose(bool isDisposing)
    {
        if (!IsDisposed && isDisposing)
        {
            IsDisposed = true;

#if DEBUG
            Super.HotReload -= ReloadUi;
#endif

            foreach (var child in InternalChildren)
            {
                if (child is IDisposable disposable)
                {
                    disposable?.Dispose();
                }
                else
                if (child is Grid grid)
                {
                    foreach (var gridChild in grid.Children)
                    {
                        if (gridChild is IDisposable disposableChild)
                        {
                            disposableChild?.Dispose();
                        }
                    }
                }
                else
                if (child is StackLayout stack)
                {
                    foreach (var stackChild in stack.Children)
                    {
                        if (stackChild is IDisposable disposableChild)
                        {
                            disposableChild?.Dispose();
                        }
                    }
                }
            }
        }

    }
}