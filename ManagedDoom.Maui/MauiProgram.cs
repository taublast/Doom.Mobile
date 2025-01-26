using DrawnUi.Maui.Draw;
using Microsoft.Extensions.Logging;
using Plugin.Maui.Audio;
using System.Numerics;

namespace ManagedDoom.Maui
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .AddAudio()
                .UseDrawnUi(new()
                {
                    UseDesktopKeyboard = true,
                    DesktopWindow = new()
                    {
                        Width = 640,
                        Height = 400,
                        //IsFixedSize = true
                        //todo disable maximize btn 
                    }
                })
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("AmazDoomRight.ttf", "FontGame");
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
        /// <summary>
        /// The game can attach to this to pause/resume
        /// </summary>
        public static EventHandler<bool> AppIsActiveChanged;

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
