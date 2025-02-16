using DrawnUi.Maui.Draw;
using Orbit.Input;

namespace ManagedDoom.Maui
{
    public partial class MainPage
    {

        public MainPage()
        {
            try
            {
                InitializeComponent();
#if IOS
                Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific.Page.SetPrefersHomeIndicatorAutoHidden(this, true);
#endif

                Console.WriteLine("HELLO FROM DOOM");
                GameControllerManager.Current.GameControllerConnected += OnGameControllerConnected;
            }
            catch (Exception e)
            {
                Super.DisplayException(this, e);
            }
        }

        private async void OnGameControllerConnected(object? sender, GameControllerConnectedEventArgs args)
        {
            await DisplayAlert(@"Game controller connected!", "Game controller connected!", "OK");
        }
    }

}
