using ManagedDoom.UserInput;
using Orbit.Input;

namespace ManagedDoom.Maui.Game;

public class GameControllerUserInput : IUserInput
{
    private Orbit.Input.GameController? _gameController;
    private int _currentIndex;

    public GameControllerUserInput()
    {
        GameControllerManager.Current.GameControllerConnected += OnGameControllerConnected; 
        _ = GameControllerManager.Current.StartDiscovery();
    }

    private void OnGameControllerConnected(object? sender, GameControllerConnectedEventArgs args)
    {
        _gameController = args.GameController;

        _gameController.ButtonChanged += (o, eventArgs) =>
        {
            if (eventArgs.ButtonName == _gameController.North.Name &&
                eventArgs.IsPressed)
            {
                _currentIndex = (_currentIndex + TicCmdButtons.WeaponShift) % (7 * TicCmdButtons.WeaponShift);
            }
        };
    }
    
    public void BuildTicCmd(TicCmd cmd)
    {
        if (_gameController is null)
        {
            return;
        }
        
        if (_gameController.RightShoulder.Trigger.Value > 0)
        {
            cmd.Buttons |= TicCmdButtons.Attack;
        }

        if (_gameController.South.Value)
        {
            cmd.Buttons |= TicCmdButtons.Use;
        }

        if (_gameController.North.Value)
        {
            cmd.Buttons |= TicCmdButtons.Change;
            cmd.Buttons |= (byte)_currentIndex;
        }
        
        cmd.AngleTurn -= (short)(_gameController.RightStick.XAxis.Value * 0x150);
        cmd.ForwardMove += (sbyte)(_gameController.LeftStick.YAxis.Value * 0x8);
        cmd.SideMove += (sbyte)(_gameController.LeftStick.XAxis.Value * 0x8);
    }

    public void Reset()
    {
        
    }

    public void GrabMouse()
    {
        
    }

    public void ReleaseMouse()
    {
        
    }

    public int MaxMouseSensitivity { get; }
    public int MouseSensitivity { get; set; }
}