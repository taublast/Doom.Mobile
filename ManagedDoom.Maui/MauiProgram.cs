//#define DEBUG_MOBILE
using DrawnUi.Draw;
using Microsoft.Extensions.Logging;
using Plugin.Maui.Audio;

namespace ManagedDoom.Maui
{
    public static class MauiProgram
    {
        /// <summary>
        /// Respect or not game aspect ratio, surprisingly Doom looks good even with aspect deformations
        /// </summary>
        public static bool KeepAspectRatio = true;

        /// <summary>
        /// Optional HUD for simulating some the keys.
        /// Mainly for a demo purpose to demonstrate the way to show a HUD.
        /// Might want to enable for smart watch..
        /// If true then Keypad will be shown while playing.
        /// </summary>
        public static bool ShowKeys = false;

        /// <summary>
        /// Use sound or not
        /// </summary>
        public static bool UseSound = true;

        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .AddAudio()
#if DEBUG_MOBILE //don't need this to compile, it's for development to simulate mobile screen on desktop
                .UseDrawnUi(new()
                {
                    UseDesktopKeyboard = true,
                    DesktopWindow = new()
                    {
                        Width = 400,
                        Height = 700,
                        IsFixedSize = true
                    }
                });
            IsMobile = true;
#else
              .UseDrawnUi(new()
              {
                  UseDesktopKeyboard = true,
                  MobileIsFullscreen = true,
                  DesktopWindow = new()
                  {
                      Width = 640,
                      Height = 400,
                  }
              });
            IsMobile = DeviceInfo.Current.Idiom == DeviceIdiom.Tablet || DeviceInfo.Current.Idiom == DeviceIdiom.Phone;
#endif

            builder.ConfigureFonts(fonts =>
                {
                    fonts.AddFont("AmazDoomRight.ttf", "FontGame");
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                })
                .ConfigureMauiHandlers(handlers =>
                {
#if IOS
                    //to force Home indicator hidden
                    handlers.AddHandler<MyPage, MyPageHandler>();
#endif
                });

            //

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
        /// <summary>
        /// The game can attach to this to pause/resume
        /// </summary>
        public static EventHandler<bool> AppIsActiveChanged;

        public static bool IsMobile { get; set; }

        /// <summary>
        /// Will be used by App.cs
        /// </summary>
        /// <param name="state"></param>
        public static void ReportAppIsActive(bool state)
        {
            AppIsActiveChanged?.Invoke(null, state);
        }

        public static bool IsDebug
        {
            get
            {
#if DEBUG
                return true;
#else
                return false;
#endif
            }
        }
    }
}
