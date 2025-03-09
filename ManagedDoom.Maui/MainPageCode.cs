using DrawnUi.Maui.Draw;
using DrawnUi.Maui.Game;
using DrawnUi.Maui.Infrastructure.Enums;
using DrawnUi.Maui.Views;
using ManagedDoom.Maui.Game;

namespace ManagedDoom.Maui;

/// <summary>
/// Replaces MainPage.xaml. It's up to use to decide which one to use.
/// </summary>
public class MainPageCode : BaseCodePage
{
    Canvas Canvas;
    private IMauiGame _game;

    public override void Build()
    {
        Title = ".NET MAUI DOOM";
        this.SetValue(Shell.FlyoutBehaviorProperty, FlyoutBehavior.Disabled);
        this.SetValue(Shell.NavBarIsVisibleProperty, false);

        Canvas?.Dispose();

        Canvas = new Canvas()
        {
            UpdateMode = UpdateMode.Constant,
            Gestures = GesturesMode.Lock,
            HardwareAcceleration = HardwareAccelerationMode.Enabled,
            VerticalOptions = LayoutOptions.Fill,
            HorizontalOptions = LayoutOptions.Fill,
            BackgroundColor = Colors.Black,

            Content = new SkiaLayout() // absolute wrapper
            {
                HorizontalOptions = LayoutOptions.Fill,
                VerticalOptions = LayoutOptions.Fill
            }
            .WithChildren(

                new MauiDoom()
                    .Adjust((c) =>
                    {
                        _game = c;
                    }),
#if DEBUG
                new SkiaLabelFps()
                {
                    Margin = new(0, 0, 4, 24),
                    VerticalOptions = LayoutOptions.End,
                    HorizontalOptions = LayoutOptions.End,
                    Rotation = -45,
                    BackgroundColor = Colors.DarkRed,
                    TextColor = Colors.White,
                    ZIndex = 100,
                }
#endif
            )
        };

        this.Content = Canvas;
    }

    protected override void Dispose(bool isDisposing)
    {
        if (isDisposing)
        {
            this.Content = null;
            Canvas?.Dispose();
        }

        base.Dispose(isDisposing);
    }
}