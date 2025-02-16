using ManagedDoom.UserInput;
using Orbit.Input;

namespace ManagedDoom.Maui.Game;

public class GameControllerUserInput : IUserInput
{
    private Orbit.Input.GameController? _gameController;
    private int _currentIndex;

    public GameControllerUserInput()
    {
        // GameControllerManager.Current.GameControllerConnected += OnGameControllerConnected; 
        // _ = GameControllerManager.Current.Initialize();
    }

    private void OnGameControllerConnected(object? sender, GameControllerConnectedEventArgs args)
    {
        _gameController = args.GameController;
        
        _gameController
            .When(
                button: "ButtonNorth",
                isPressed: _ =>
                {
                    _currentIndex = (_currentIndex + TicCmdButtons.WeaponShift) % (7 * TicCmdButtons.WeaponShift);
                })
            .When(
                button: "RightTrigger",
                changesValue: f =>
                {
                    
                });
    }
    
    public void BuildTicCmd(TicCmd cmd)
    {
        if (_gameController is null)
        {
            return;
        }
        
        if (_gameController.RightShoulder.Trigger > 0)
        {
            cmd.Buttons |= TicCmdButtons.Attack;
        }

        if (_gameController.ButtonSouth)
        {
            cmd.Buttons |= TicCmdButtons.Use;
        }

        if (_gameController.ButtonNorth)
        {
            cmd.Buttons |= TicCmdButtons.Change;
            cmd.Buttons |= (byte)_currentIndex;
        }
        
        cmd.AngleTurn -= (short)(_gameController.RightStick.XAxis * 0x150);
        cmd.ForwardMove += (sbyte)(_gameController.LeftStick.YAxis * 0x8);
        cmd.SideMove += (sbyte)(_gameController.LeftStick.XAxis * 0x8);
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