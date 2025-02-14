using Foundation;

using GameController;

namespace Orbit.Input;

public partial class GameControllerManager
{
    private GameControllerManager()
    {
        GCController.Notifications.ObserveDidConnect(ConnectToController);
    }
    
    public partial async Task Initialize()
    {
        await GCController.StartWirelessControllerDiscoveryAsync();
    }
    
    private void ConnectToController(object? sender, NSNotificationEventArgs e)
    {
        if (e.Notification.Object is GCController controller)
        {
            var gameController = new GameController(controller);
            gameControllers.Add(gameController);
            OnGameControllerConnected(gameController);
        }
    }
}