using ManagedDoom.UserInput;

namespace ManagedDoom.Maui.Game;

public class AggregatedUserInput : IUserInput
{
    private readonly IReadOnlyList<IUserInput> _userInputs;
    
    public AggregatedUserInput(IReadOnlyList<IUserInput> userInputs)
    {
        _userInputs = userInputs;
    }
    
    public void BuildTicCmd(TicCmd cmd)
    {
        cmd.Clear();
        
        foreach (var userInput in _userInputs)
        {
            userInput.BuildTicCmd(cmd);
        }
    }

    public void Reset()
    {
        foreach (var userInput in _userInputs)
        {
            userInput.Reset();
        }
    }

    public void GrabMouse()
    {
        foreach (var userInput in _userInputs)
        {
            userInput.GrabMouse();
        }
    }

    public void ReleaseMouse()
    {
        foreach (var userInput in _userInputs)
        {
            userInput.ReleaseMouse();
        }
    }

    public int MaxMouseSensitivity { get; }
    public int MouseSensitivity { get; set; }
}