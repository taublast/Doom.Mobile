using ManagedDoom;
using ManagedDoom.UserInput;
using System.Diagnostics;
using Windows.System;
#if WINDOWS || __MACCATALYST__ || __MACOS__
using Microsoft.UI.Xaml.Input;
using Windows.System;
#endif

namespace UnoDoom.Game;

public class UnoUserInput : IUserInput, IDisposable
{
    private bool[] weaponKeys;
    private int turnHeld;
    private Dictionary<DoomKey, EventTimestamp> Pressed = new();
    private readonly Config _config;
    private bool mouseGrabbed = false;
    private float mouseDeltaX = 0;
    private float mouseDeltaY = 0;

    public UnoUserInput(Config config, bool useMouse)
    {
        _config = config;
        weaponKeys = new bool[7];
        mouseGrabbed = useMouse;
    }

    public void SetKeyStatus(EventType type, DoomKey doomKey, Doom doom, EventTimestamp currentTime)
    {
        var pressed = EventTimestamp.Empty;
        if (type == EventType.KeyDown)
            pressed = currentTime;

        Pressed[doomKey] = pressed;
        
        Debug.WriteLine($"[EVENT] {type} {doomKey} at {Pressed[doomKey].Frame}");
        doom.PostEvent(new DoomEvent(type, doomKey));
    }

    public static DoomKey VirtualKeyToDoom(VirtualKey key)
    {
        switch (key)
        {
            case VirtualKey.Space: return DoomKey.Space;
            case (VirtualKey)188: return DoomKey.Comma; // VK_OEM_COMMA
            case (VirtualKey)189: return DoomKey.Subtract; // VK_OEM_MINUS
            case (VirtualKey)190: return DoomKey.Period; // VK_OEM_PERIOD
            case (VirtualKey)191: return DoomKey.Slash; // VK_OEM_2
            case VirtualKey.Number0: return DoomKey.Num0;
            case VirtualKey.Number1: return DoomKey.Num1;
            case VirtualKey.Number2: return DoomKey.Num2;
            case VirtualKey.Number3: return DoomKey.Num3;
            case VirtualKey.Number4: return DoomKey.Num4;
            case VirtualKey.Number5: return DoomKey.Num5;
            case VirtualKey.Number6: return DoomKey.Num6;
            case VirtualKey.Number7: return DoomKey.Num7;
            case VirtualKey.Number8: return DoomKey.Num8;
            case VirtualKey.Number9: return DoomKey.Num9;
            case (VirtualKey)186: return DoomKey.Semicolon; // VK_OEM_1
            case (VirtualKey)187: return DoomKey.Equal; // VK_OEM_PLUS
            case VirtualKey.A: return DoomKey.A;
            case VirtualKey.B: return DoomKey.B;
            case VirtualKey.C: return DoomKey.C;
            case VirtualKey.D: return DoomKey.D;
            case VirtualKey.E: return DoomKey.E;
            case VirtualKey.F: return DoomKey.F;
            case VirtualKey.G: return DoomKey.G;
            case VirtualKey.H: return DoomKey.H;
            case VirtualKey.I: return DoomKey.I;
            case VirtualKey.J: return DoomKey.J;
            case VirtualKey.K: return DoomKey.K;
            case VirtualKey.L: return DoomKey.L;
            case VirtualKey.M: return DoomKey.M;
            case VirtualKey.N: return DoomKey.N;
            case VirtualKey.O: return DoomKey.O;
            case VirtualKey.P: return DoomKey.P;
            case VirtualKey.Q: return DoomKey.Q;
            case VirtualKey.R: return DoomKey.R;
            case VirtualKey.S: return DoomKey.S;
            case VirtualKey.T: return DoomKey.T;
            case VirtualKey.U: return DoomKey.U;
            case VirtualKey.V: return DoomKey.V;
            case VirtualKey.W: return DoomKey.W;
            case VirtualKey.X: return DoomKey.X;
            case VirtualKey.Y: return DoomKey.Y;
            case VirtualKey.Z: return DoomKey.Z;
            case (VirtualKey)219: return DoomKey.LBracket; // VK_OEM_4
            case (VirtualKey)221: return DoomKey.RBracket; // VK_OEM_6
            case VirtualKey.Escape: return DoomKey.Escape;
            case VirtualKey.Enter: return DoomKey.Enter;
            case VirtualKey.Tab: return DoomKey.Tab;
            case VirtualKey.Back: return DoomKey.Backspace;
            case VirtualKey.Insert: return DoomKey.Insert;
            case VirtualKey.Delete: return DoomKey.Delete;
            case VirtualKey.Right: return DoomKey.Right;
            case VirtualKey.Left: return DoomKey.Left;
            case VirtualKey.Down: return DoomKey.Down;
            case VirtualKey.Up: return DoomKey.Up;
            case VirtualKey.PageUp: return DoomKey.PageUp;
            case VirtualKey.PageDown: return DoomKey.PageDown;
            case VirtualKey.Home: return DoomKey.Home;
            case VirtualKey.End: return DoomKey.End;
            case VirtualKey.F1: return DoomKey.F1;
            case VirtualKey.F2: return DoomKey.F2;
            case VirtualKey.F3: return DoomKey.F3;
            case VirtualKey.F4: return DoomKey.F4;
            case VirtualKey.F5: return DoomKey.F5;
            case VirtualKey.F6: return DoomKey.F6;
            case VirtualKey.F7: return DoomKey.F7;
            case VirtualKey.F8: return DoomKey.F8;
            case VirtualKey.F9: return DoomKey.F9;
            case VirtualKey.F10: return DoomKey.F10;
            case VirtualKey.F11: return DoomKey.F11;
            case VirtualKey.F12: return DoomKey.F12;
            case VirtualKey.Shift: return DoomKey.LShift;
            case VirtualKey.Control: return DoomKey.LControl;
            case VirtualKey.Menu: return DoomKey.LAlt;
            case VirtualKey.Pause: return DoomKey.Pause;
            default: return DoomKey.Unknown;
        }
    }

    public void Update(Doom doom, EventTimestamp currentTime)
    {
        // Process any pending inputs
    }

    private bool IsPressed(DoomKey key)
    {
        if (Pressed.TryGetValue(key, out var pressed))
        {
            if (!pressed.IsEmpty)
                return true;
        }
        return false;
    }

    private bool IsPressed(KeyBinding keyBinding)
    {
        foreach (var key in keyBinding.Keys)
        {
            if (Pressed.TryGetValue(key, out var pressed))
            {
                if (!pressed.IsEmpty)
                    return true;
            }
        }
        return false;
    }

    public void BuildTicCmd(TicCmd cmd)
    {
        var keyForward = IsPressed(_config.key_forward);
        var keyBackward = IsPressed(_config.key_backward);
        var keyStrafeLeft = IsPressed(_config.key_strafeleft);
        var keyStrafeRight = IsPressed(_config.key_straferight);
        var keyTurnLeft = IsPressed(_config.key_turnleft);
        var keyTurnRight = IsPressed(_config.key_turnright);
        var keyFire = IsPressed(_config.key_fire);
        var keyUse = IsPressed(_config.key_use);
        var keyRun = IsPressed(_config.key_run);
        var keyStrafe = IsPressed(_config.key_strafe);

        weaponKeys[0] = IsPressed(DoomKey.Num1);
        weaponKeys[1] = IsPressed(DoomKey.Num2);
        weaponKeys[2] = IsPressed(DoomKey.Num3);
        weaponKeys[3] = IsPressed(DoomKey.Num4);
        weaponKeys[4] = IsPressed(DoomKey.Num5);
        weaponKeys[5] = IsPressed(DoomKey.Num6);
        weaponKeys[6] = IsPressed(DoomKey.Num7);

        cmd.Clear();

        var strafe = keyStrafe;
        var speed = keyRun ? 1 : 0;
        var forward = 0;
        var side = 0;

        if (_config.game_alwaysrun)
        {
            speed = 1 - speed;
        }

        if (keyTurnLeft || keyTurnRight)
        {
            turnHeld++;
        }
        else
        {
            turnHeld = 0;
        }

        int turnSpeed;
        if (turnHeld < PlayerBehavior.SlowTurnTics)
        {
            turnSpeed = 2;
        }
        else
        {
            turnSpeed = speed;
        }

        if (strafe)
        {
            if (keyTurnRight)
            {
                side += PlayerBehavior.SideMove[speed];
            }
            if (keyTurnLeft)
            {
                side -= PlayerBehavior.SideMove[speed];
            }
        }
        else
        {
            if (keyTurnRight)
            {
                cmd.AngleTurn -= (short)PlayerBehavior.AngleTurn[turnSpeed];
            }
            if (keyTurnLeft)
            {
                cmd.AngleTurn += (short)PlayerBehavior.AngleTurn[turnSpeed];
            }
        }

        if (keyForward)
        {
            forward += PlayerBehavior.ForwardMove[speed];
        }
        if (keyBackward)
        {
            forward -= PlayerBehavior.ForwardMove[speed];
        }

        if (keyStrafeLeft)
        {
            side -= PlayerBehavior.SideMove[speed];
        }
        if (keyStrafeRight)
        {
            side += PlayerBehavior.SideMove[speed];
        }

        if (keyFire)
        {
            cmd.Buttons |= TicCmdButtons.Attack;
        }

        if (keyUse)
        {
            cmd.Buttons |= TicCmdButtons.Use;
        }

        // Check weapon keys
        for (var i = 0; i < weaponKeys.Length; i++)
        {
            if (weaponKeys[i])
            {
                cmd.Buttons |= TicCmdButtons.Change;
                cmd.Buttons |= (byte)(i << TicCmdButtons.WeaponShift);
                break;
            }
        }

        var ms = 0.5F * _config.mouse_sensitivity;
        var mx = (int)MathF.Round(ms * mouseDeltaX);
        var my = (int)MathF.Round(ms * -mouseDeltaY);

        forward += my;
        if (strafe)
        {
            side += mx * 2;
        }
        else
        {
            cmd.AngleTurn -= (short)(mx * 0x8);
        }

        if (forward > PlayerBehavior.MaxMove)
        {
            forward = PlayerBehavior.MaxMove;
        }
        else if (forward < -PlayerBehavior.MaxMove)
        {
            forward = -PlayerBehavior.MaxMove;
        }
        if (side > PlayerBehavior.MaxMove)
        {
            side = PlayerBehavior.MaxMove;
        }
        else if (side < -PlayerBehavior.MaxMove)
        {
            side = -PlayerBehavior.MaxMove;
        }

        cmd.ForwardMove += (sbyte)forward;
        cmd.SideMove += (sbyte)side;

        mouseDeltaX = 0;
        mouseDeltaY = 0;
    }

    public void Reset()
    {
        Pressed.Clear();
        turnHeld = 0;
        mouseDeltaX = 0;
        mouseDeltaY = 0;
    }

    public void Dispose()
    {
    }

    public void GrabMouse()
    {
        mouseGrabbed = true;
    }

    public void ReleaseMouse()
    {
        mouseGrabbed = false;
    }

    public int MaxMouseSensitivity => 9;

    public int MouseSensitivity
    {
        get => _config.mouse_sensitivity;
        set => _config.mouse_sensitivity = value;
    }
}
