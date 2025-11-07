using DrawnUi.Draw;
using AppoMobi.Maui.Gestures;
using DrawnUi.Gaming;

namespace ManagedDoom.Maui.Game
{

    /// <summary>
    /// Optional HUD for simulating some the keys. Mainly for a demo purpose.
    /// You can activate it with MauiProgram.ShowKeys = false;
    /// </summary>
    public class Keypad : SkiaLayout
    {

        public class KeypadButton : SkiaShape
        {
            private const double BtnSize = 40.0;
            private const double BtnCorners = 12.0;
            private Color BtnColor = Color.Parse("#FEfEFE");

            public KeypadButton(IMauiGame game, string caption, MauiKey key, bool alt = false)
            {
                UseCache = SkiaCacheType.Image;
                CornerRadius = BtnCorners;
                WidthRequest = BtnSize;
                LockRatio = 1; //make height same size
                BackgroundColor = BtnColor;
                Padding = 0;

                Children = new List<SkiaControl>()
                {
                    new SkiaLayout()
                    {
                        HorizontalOptions = LayoutOptions.Fill,
                        VerticalOptions = LayoutOptions.Fill,
                        Children = new List<SkiaControl>()
                        {
                            new SkiaImage()
                            {
                                Source = "ui.png",
                                HorizontalOptions = LayoutOptions.Fill,
                                VerticalOptions = LayoutOptions.Fill,
                                Aspect = TransformAspect.FitFill
                            },
                            new SkiaLabel()
                            {
                                Text = caption,
                                TextColor = Colors.White,
                                DropShadowColor = Color.Parse("#66000000"),
                                FontFamily = "FontGame",
                                Opacity = 0.75,
                                TranslationY = 4, //this fan-made font needs adjusting for centering
                                //FontAttributes = FontAttributes.Bold,
                                FontSize = 14,
                                HorizontalOptions = LayoutOptions.Center,
                                VerticalOptions = LayoutOptions.Center
                            }
                        }
                    }
                };



                OnGestures = (parameters, info) =>
                {
                    if (game != null)
                    {
                        if (parameters.Type == TouchActionResult.Down)
                        {
                            if (alt)
                            {
                                game.OnKeyDown(MauiKey.AltLeft);
                            }
                            game.OnKeyDown(key);
                            return this;
                        }
                        else if (parameters.Type == TouchActionResult.Up)
                        {
                            if (alt)
                            {
                                game.OnKeyUp(MauiKey.AltLeft);
                            }
                            game.OnKeyUp(key);
                            return this;
                        }
                    }

                    return null;
                };
            }

        }

        public Keypad(IMauiGame game)
        {
            try
            {
                VerticalOptions = LayoutOptions.Center;
                HorizontalOptions = LayoutOptions.Fill;
                Margin = new(24);

                UseCache = SkiaCacheType.Operations;

                Children = new List<SkiaControl>()
                {
                    new SkiaLayout()
                    {
                        HorizontalOptions = LayoutOptions.Start,
                        Type = LayoutType.Row,
                        Spacing = 16,
                        Children = new List<SkiaControl>()
                        {
                            new KeypadButton(game, "Left", MauiKey.ArrowLeft, true),
                            new KeypadButton(game, "Right", MauiKey.ArrowRight, true),
                        }
                    },
                    new SkiaLayout()
                    {
                        HorizontalOptions = LayoutOptions.End,
                        Type = LayoutType.Row,
                        Spacing = 16,
                        Children = new List<SkiaControl>()
                        {
                            new KeypadButton(game, "Use", MauiKey.Space),
                           // new KeypadButton(game, "Fire", MauiKey.ControlLeft),
                        }
                    }

                };
                ;
            }
            catch (Exception e)
            {
                Super.DisplayException(this, e);
            }
        }



    }
}
