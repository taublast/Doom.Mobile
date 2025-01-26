using AppoMobi.Specials;
using DrawnUi.Maui.Draw;
using static ManagedDoom.CommandLineArgs;

namespace ManagedDoom.Maui
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            MainPage = new MainPage();
        }

        protected override void OnStart()
        {
            base.OnStart();

            Tasks.StartDelayed(TimeSpan.FromSeconds(3), () =>
            {
                Dispatcher.Dispatch(() =>
                {
                    DeviceDisplay.Current.KeepScreenOn = true;
                });
            });
        }

        protected override void OnSleep()
        {
            base.OnSleep();

            Dispatcher.Dispatch(() =>
            {
                DeviceDisplay.Current.KeepScreenOn = false;
            });

            MauiProgram.ReportAppIsActive(false);
        }

        protected override void OnResume()
        {
            base.OnResume();

            Dispatcher.Dispatch(() =>
            {
                DeviceDisplay.Current.KeepScreenOn = true;
            });

            MauiProgram.ReportAppIsActive(true);
        }

    }
}