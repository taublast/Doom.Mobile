using DrawnUi.Maui.Draw;
using SkiaSharp;
using System.Windows.Input;

namespace ManagedDoom.Maui.Game;

public partial class WeaponsView
{
    private readonly Action<int> _callback;

    public WeaponsView(Action<int> callback)
    {
        try
        {
            InitializeComponent();
            _callback = callback;
        }
        catch (Exception e)
        {
            Super.DisplayException(this, e);
        }
    }

    public ICommand TappedCommand => new Command((ctx) =>
    {
        if (ctx is string key)
        {
            _callback?.Invoke(int.Parse(key));
        }
    });
}