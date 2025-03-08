using AppoMobi.Maui.Gestures;
using DrawnUi.Maui.Draw;
using ManagedDoom.UserInput;
using SkiaSharp;
using System.Diagnostics;
using System.Numerics;
using static ManagedDoom.Maui.Game.MauiDoom;

namespace ManagedDoom.Maui.Game;

public class MauiUserInput : IUserInput, IDisposable
{
    private bool[] weaponKeys;
    private int turnHeld;

    private Dictionary<DoomKey, EventTimestamp> Pressed = new();
    private readonly Config _config;

    /// <summary>
    /// Set the status of a key, Down or Up.
    /// If it is an event then fire this event to Doom engine, otherwise just set the key status.
    /// </summary>
    /// <param name="type"></param>
    /// <param name="doomKey"></param>
    /// <param name="doom"></param>
    /// <param name="currentTime"></param>
    /// <param name="isEvent"></param>
    public void SetKeyStatus(EventType type, DoomKey doomKey, Doom doom, EventTimestamp currentTime)
    {
        var pressed = EventTimestamp.Empty;
        if (type == EventType.KeyDown)
            pressed = currentTime;

        var needTrigger = true;
        //if (Pressed.TryGetValue(doomKey, out var set))
        //{
        //    if (set.IsEmpty)
        //        needTrigger = false;
        //}

        Pressed[doomKey] = pressed;

        if (needTrigger)
        {
            Debug.WriteLine($"[EVENT] {type} {doomKey} at {Pressed[doomKey].Frame}");
            doom.PostEvent(new DoomEvent(type, doomKey));
        }
        else
        {
            Debug.WriteLine($"[SKIPPED EVENT] {type} {doomKey}");
        }
    }

    public static DoomKey KeyToDoom(MauiKey silkKey)
    {
        switch (silkKey)
        {
        case MauiKey.Space: return DoomKey.Space;
        // case MauiKey.Apostrophe: return DoomKey.Apostrophe;
        case MauiKey.Comma: return DoomKey.Comma;
        case MauiKey.Minus: return DoomKey.Subtract;
        case MauiKey.Period: return DoomKey.Period;
        case MauiKey.Slash: return DoomKey.Slash;
        case MauiKey.Numpad0: return DoomKey.Num0;
        // case MauiKey.D0: return DoomKey.D0;
        case MauiKey.Numpad1: return DoomKey.Num1;
        case MauiKey.Numpad2: return DoomKey.Num2;
        case MauiKey.Numpad3: return DoomKey.Num3;
        case MauiKey.Numpad4: return DoomKey.Num4;
        case MauiKey.Numpad5: return DoomKey.Num5;
        case MauiKey.Numpad6: return DoomKey.Num6;
        case MauiKey.Numpad7: return DoomKey.Num7;
        case MauiKey.Numpad8: return DoomKey.Num8;
        case MauiKey.Numpad9: return DoomKey.Num9;
        case MauiKey.Semicolon: return DoomKey.Semicolon;
        case MauiKey.Equal: return DoomKey.Equal;
        case MauiKey.KeyA: return DoomKey.A;
        case MauiKey.KeyB: return DoomKey.B;
        case MauiKey.KeyC: return DoomKey.C;
        case MauiKey.KeyD: return DoomKey.D;
        case MauiKey.KeyE: return DoomKey.E;
        case MauiKey.KeyF: return DoomKey.F;
        case MauiKey.KeyG: return DoomKey.G;
        case MauiKey.KeyH: return DoomKey.H;
        case MauiKey.KeyI: return DoomKey.I;
        case MauiKey.KeyJ: return DoomKey.J;
        case MauiKey.KeyK: return DoomKey.K;
        case MauiKey.KeyL: return DoomKey.L;
        case MauiKey.KeyM: return DoomKey.M;
        case MauiKey.KeyN: return DoomKey.N;
        case MauiKey.KeyO: return DoomKey.O;
        case MauiKey.KeyP: return DoomKey.P;
        case MauiKey.KeyQ: return DoomKey.Q;
        case MauiKey.KeyR: return DoomKey.R;
        case MauiKey.KeyS: return DoomKey.S;
        case MauiKey.KeyT: return DoomKey.T;
        case MauiKey.KeyU: return DoomKey.U;
        case MauiKey.KeyV: return DoomKey.V;
        case MauiKey.KeyW: return DoomKey.W;
        case MauiKey.KeyX: return DoomKey.X;
        case MauiKey.KeyY: return DoomKey.Y;
        case MauiKey.KeyZ: return DoomKey.Z;
        case MauiKey.BracketLeft: return DoomKey.LBracket;
        case MauiKey.Backslash: return DoomKey.Backslash;
        case MauiKey.BracketRight: return DoomKey.RBracket;
        // case MauiKey.GraveAccent: return DoomKey.GraveAccent;
        // case MauiKey.World1: return DoomKey.World1;
        // case MauiKey.World2: return DoomKey.World2;
        case MauiKey.Escape: return DoomKey.Escape;
        case MauiKey.Enter: return DoomKey.Enter;
        case MauiKey.Tab: return DoomKey.Tab;
        case MauiKey.Backspace: return DoomKey.Backspace;
        case MauiKey.Insert: return DoomKey.Insert;
        case MauiKey.Delete: return DoomKey.Delete;
        case MauiKey.ArrowRight: return DoomKey.Right;
        case MauiKey.ArrowLeft: return DoomKey.Left;
        case MauiKey.ArrowDown: return DoomKey.Down;
        case MauiKey.ArrowUp: return DoomKey.Up;
        case MauiKey.PageUp: return DoomKey.PageUp;
        case MauiKey.PageDown: return DoomKey.PageDown;
        case MauiKey.Home: return DoomKey.Home;
        case MauiKey.End: return DoomKey.End;
        // case MauiKey.CapsLock: return DoomKey.CapsLock;
        // case MauiKey.ScrollLock: return DoomKey.ScrollLock;
        // case MauiKey.NumLock: return DoomKey.NumLock;
        // case MauiKey.PrintScreen: return DoomKey.PrintScreen;
        case MauiKey.Pause: return DoomKey.Pause;
        case MauiKey.F1: return DoomKey.F1;
        case MauiKey.F2: return DoomKey.F2;
        case MauiKey.F3: return DoomKey.F3;
        case MauiKey.F4: return DoomKey.F4;
        case MauiKey.F5: return DoomKey.F5;
        case MauiKey.F6: return DoomKey.F6;
        case MauiKey.F7: return DoomKey.F7;
        case MauiKey.F8: return DoomKey.F8;
        case MauiKey.F9: return DoomKey.F9;
        case MauiKey.F10: return DoomKey.F10;
        case MauiKey.F11: return DoomKey.F11;
        case MauiKey.F12: return DoomKey.F12;
        //case MauiKey.F13: return DoomKey.F13; //todo?
        //case MauiKey.F14: return DoomKey.F14;//todo?
        //case MauiKey.F15: return DoomKey.F15;//todo?
        // case MauiKey.F16: return DoomKey.F16;
        // case MauiKey.F17: return DoomKey.F17;
        // case MauiKey.F18: return DoomKey.F18;
        // case MauiKey.F19: return DoomKey.F19;
        // case MauiKey.F20: return DoomKey.F20;
        // case MauiKey.F21: return DoomKey.F21;
        // case MauiKey.F22: return DoomKey.F22;
        // case MauiKey.F23: return DoomKey.F23;
        // case MauiKey.F24: return DoomKey.F24;
        // case MauiKey.F25: return DoomKey.F25;
        //case MauiKey.Numpad0: return DoomKey.Numpad0; //todo check keypad insted of numpad?
        //case MauiKey.Numpad1: return DoomKey.Numpad1;
        //case MauiKey.Numpad2: return DoomKey.Numpad2;
        //case MauiKey.Numpad3: return DoomKey.Numpad3;
        //case MauiKey.Numpad4: return DoomKey.Numpad4;
        //case MauiKey.Numpad5: return DoomKey.Numpad5;
        //case MauiKey.Keypad6: return DoomKey.Numpad6;
        //case MauiKey.Keypad7: return DoomKey.Numpad7;
        //case MauiKey.Keypad8: return DoomKey.Numpad8;
        //case MauiKey.Keypad9: return DoomKey.Numpad9;
        // case MauiKey.KeypadDecimal: return DoomKey.Decimal;
        case MauiKey.NumpadDivide: return DoomKey.Divide;
        case MauiKey.NumpadMultiply: return DoomKey.Multiply;
        case MauiKey.NumpadSubtract: return DoomKey.Subtract;
        case MauiKey.NumpadAdd: return DoomKey.Add;
        //case MauiKey.Enter return DoomKey.Enter;
        //case MauiKey.Equal: return DoomKey.Equal;
        case MauiKey.ShiftLeft: return DoomKey.LShift;
        case MauiKey.ControlLeft: return DoomKey.LControl;
        case MauiKey.AltLeft: return DoomKey.LAlt;
        // case MauiKey.SuperLeft: return DoomKey.SuperLeft;
        case MauiKey.ShiftRight: return DoomKey.RShift;
        case MauiKey.ControlRight: return DoomKey.RControl;
        case MauiKey.AltRight: return DoomKey.RAlt;
        // case MauiKey.SuperRight: return DoomKey.SuperRight;
        case MauiKey.ContextMenu: return DoomKey.Menu;
        default: return DoomKey.Unknown;
        }
    }


    public MauiUserInput(Config config, bool useMouse, Func<UiCommand, bool> callbackInputForUi)
    {
        _config = config;
        _desktop = !MauiProgram.IsMobile;
        _calbackUi = callbackInputForUi;

        weaponKeys = new bool[7];
        turnHeld = 0;

        _mouse = new VirtualMouse();
        mouseGrabbed = false;

        //todo move this to config static:

        if (config.video_highresolution)
        {
            _doomViewport = new SKRect(0, 0, 400, 640);
        }
        else
        {
            _doomViewport = new SKRect(0, 0, 200, 320);
        }

    }

    private bool IsPressed(DoomKey key)
    {
        if (Pressed.TryGetValue(key, out var pressed))
        {
            if (pressed.HasValue)
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
                if (pressed.HasValue)
                    return true;
            }
        }

        if (mouseGrabbed)
        {
            foreach (var mouseButton in keyBinding.MouseButtons)
            {
                if (mouseButton == DoomMouseButton.Mouse1 && _pressedFire.HasValue) return true;
                if (mouseButton == DoomMouseButton.Mouse2 && _pressedUse.HasValue) return true;
            }
        }

        return false;
    }

    /// <summary>
    /// This method is invoked by Doom engine to build a TicCmd object.
    /// It contains data about what keys are pressed right now and mouse movement.
    /// </summary>
    /// <param name="cmd"></param>
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

        // Check weapon keys.
        for (var i = 0; i < weaponKeys.Length; i++)
        {
            if (weaponKeys[i])
            {
                cmd.Buttons |= TicCmdButtons.Change;
                cmd.Buttons |= (byte)(i << TicCmdButtons.WeaponShift);
                break;
            }
        }

        UpdateMouse();

        var ms = 0.5F * _config.mouse_sensitivity;

        if (!_desktop)
        {
            ms *= 2;
        }

        var mx = (int)MathF.Round(ms * mouseDeltaX);
        var my = (int)MathF.Round(ms * -mouseDeltaY);

        if (!_desktop)
        {
            my *= 2;
        }

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
    }

    public void Reset()
    {
        if (_mouse == null)
        {
            return;
        }

        mouseX = _mouse.Position.X;
        mouseY = _mouse.Position.Y;
        mousePrevX = mouseX;
        mousePrevY = mouseY;
        mouseDeltaX = 0;
        mouseDeltaY = 0;
    }

    private void UpdateMouse()
    {
        if (_mouse == null)
        {
            return;
        }

        if (mouseGrabbed)
        {
            mousePrevX = mouseX;
            mousePrevY = mouseY;
            mouseX = _mouse.Position.X;
            mouseY = _mouse.Position.Y;
            mouseDeltaX = mouseX - mousePrevX;
            mouseDeltaY = mouseY - mousePrevY;

            if (_config.mouse_disableyaxis)
            {
                mouseDeltaY = 0;
            }
        }
    }

    public void GrabMouse()
    {
        if (_mouse == null)
        {
            return;
        }

        if (!mouseGrabbed)
        {
            //_mouse.Cursor.CursorMode = CursorMode.Raw;
            mouseGrabbed = true;
            mouseX = _mouse.Position.X;
            mouseY = _mouse.Position.Y;
            mousePrevX = mouseX;
            mousePrevY = mouseY;
            mouseDeltaX = 0;
            mouseDeltaY = 0;
        }
    }


    public void ReleaseMouse()
    {
        if (_mouse == null)
        {
            return;
        }

        if (mouseGrabbed)
        {
            //_mouse.Cursor.CursorMode = CursorMode.Normal;
            _mouse.Position = new Vector2(_window.Size.Width - 10, _window.Size.Height - 10);
            Debug.WriteLine($"PAN RESET {_mouse.Position.X} {_mouse.Position.Y}");
            mouseGrabbed = false;
        }
    }

    public int MaxMouseSensitivity
    {
        get
        {
            return 15;
        }
    }

    public int MouseSensitivity
    {
        get
        {
            return _config.mouse_sensitivity;
        }

        set
        {
            _config.mouse_sensitivity = value;
        }
    }

    public void Dispose()
    {
        _doom = null;
    }

    private bool mouseGrabbed;
    private float mouseX;
    private float mouseY;
    private float mousePrevX;
    private float mousePrevY;
    private float mouseDeltaX;
    private float mouseDeltaY;

    //we need to simulate keys to control UI Menu with touch gestures

    void SetMoveLeft(bool value, Doom doom, EventTimestamp currentTime)
    {
        if (_moveLeft != value)
        {
            _moveLeft = value;
            var type = value ? EventType.KeyDown : EventType.KeyUp;
            SetKeyStatus(type, DoomKey.Left, doom, currentTime);
        }
    }

    void SetMoveRight(bool value, Doom doom, EventTimestamp currentTime)
    {
        if (_moveRight != value)
        {
            _moveRight = value;
            var type = value ? EventType.KeyDown : EventType.KeyUp;
            SetKeyStatus(type, DoomKey.Right, doom, currentTime);
        }
    }

    void SetMoveUp(bool value, Doom doom, EventTimestamp currentTime)
    {
        if (_moveUp != value)
        {
            _moveUp = value;
            var type = value ? EventType.KeyDown : EventType.KeyUp;
            SetKeyStatus(type, DoomKey.Up, doom, currentTime);
        }
    }

    void SetMoveDown(bool value, Doom doom, EventTimestamp currentTime)
    {
        if (_moveDown != value)
        {
            _moveDown = value;
            var type = value ? EventType.KeyDown : EventType.KeyUp;
            SetKeyStatus(type, DoomKey.Down, doom, currentTime);
        }
    }

    private bool _moveLeft;
    private bool _moveRight;

    private EventTimestamp _pressedFire = EventTimestamp.Empty;
    private EventTimestamp _pressedUse = EventTimestamp.Empty;

    bool _isPressed;
    private PointF _lastDown;
    private readonly VirtualMouse _mouse;
    private bool _wasPanning;
    private readonly SKRect _doomViewport;
    private SKRect _window;
    const double thresholdNotPanning = 1.0;

    /// <summary>
    /// Since ProcessGestures is invoked only when we have gestures detected
    /// and they can run several times per one frame, we need a method that would
    /// be executed just onces per frame to update everything.
    /// </summary>
    public void Update(Doom doom, EventTimestamp timestamp)
    {
        if (_pressedFire.Expired(timestamp, 50))
        {
            _pressedFire = EventTimestamp.Empty; //release left mouse button (fire)  
        }
        if (_pressedUse.Expired(timestamp, 50))
        {
            _pressedUse = EventTimestamp.Empty; ; //release right mouse button 
        }

        AutoReleaseKeys(doom, timestamp);
    }

    public void Attach(Doom doom)
    {
        _doom = doom;
    }

    private int panningFingers;

    /// <summary>
    /// Beware this can be invoked several times per frame.
    /// </summary>
    /// <param name="args"></param>
    /// <param name="apply"></param>
    /// <param name="scale"></param>
    /// <param name="window">Area where this control is capturing gestures and is rendered</param>
    /// <param name="viewport">This is the area where DOOM is rendered</param>
    /// <param name="frame"></param>
    /// <param name="doom"></param>
    /// <returns></returns>
    public bool ProcessGestures(SkiaGesturesParameters args, GestureEventProcessingInfo apply, float scale,
        SKRect window, SKRect viewport, long frame, Config config)
    {
        _window = window;
        _lastFrame = frame;

        if (_calbackUi == null)
            return false;

        var currentTime = new EventTimestamp(frame);

        var velocityX = (float)(args.Event.Distance.Velocity.X / scale);
        var velocityY = (float)(args.Event.Distance.Velocity.Y / scale);

        //Debug.WriteLine($"{args.Type} {args.Event.NumberOfTouches} {args.Event.Location.X}");

        if (args.Type == TouchActionResult.Panning)
        {
            if (args.Event.NumberOfTouches > 1)
            {
                panningFingers = args.Event.NumberOfTouches;
                return true;
            }
            else
            {
                if (panningFingers > 1)
                {
                    panningFingers = 1;
                    return true;
                }
            }

            panningFingers = 1;
            _wasPanning = true;

            var distance = args.Event.Distance.Delta;

            if (_doom.IsCapturingMouse)
            {
                //Debug.WriteLine($"PAN ++ {distance.X} {distance.Y}");
                _mouse.Position = new Vector2(_mouse.Position.X + distance.X, _mouse.Position.Y + distance.Y);
            }
            else
            {
                var velocityThreshold = 300;
                bool vertical = false;

                //up/down keys for menu
                if (velocityY < -velocityThreshold)
                {
                    SetMoveUp(true, _doom, currentTime);
                }
                else if (velocityY > velocityThreshold)
                {
                    SetMoveDown(true, _doom, currentTime);
                }

                //left/right keys for menu
                if (!_moveUp && !_moveDown)
                {
                    if (velocityX < -velocityThreshold)
                    {
                        SetMoveLeft(true, _doom, currentTime);
                    }
                    else if (velocityX > velocityThreshold)
                    {
                        SetMoveRight(true, _doom, currentTime);
                    }
                }
            }
            return true;
        }

        if (args.Type == TouchActionResult.Down)
        {
            panningFingers = 0;
            _lastDown = args.Event.Location;
            _wasPanning = false;
            _isPressed = true;
            _lastDownTime = currentTime.Timestamp;
        }

        if (args.Type == TouchActionResult.Up)
        {
            panningFingers = 0;
            var moved = Math.Abs(_lastDown.X - args.Event.Location.X);
            if (_isPressed && moved < thresholdNotPanning * scale)
            {
                if (currentTime.Timestamp - _lastDownTime < 500) //tap detected
                {
                    if (_doom.Game.World is { AutoMap.Visible: true })
                    {
                        //close map
                        SetKeyStatus(EventType.KeyDown, DoomKey.Tab, _doom, currentTime);
                    }
                    else
                    {
                        var reScale = scale * viewport.Height / 400.0f;
                        var pxHotspotHeight = 35 * reScale;
                        var avatarWidth = 36 * reScale;

                        bool IsInsideLeftBottomCorner(PointF point, SKRect rect)
                        {
                            return point.X < rect.Left + 100 * scale && point.Y > rect.Bottom - pxHotspotHeight;
                        }

                        bool IsInsideAvatar(PointF point, SKRect rect)
                        {
                            if (point.Y > rect.Bottom - pxHotspotHeight)
                            {
                                var navbarMiddle = window.Width / 2.0f;
                                return point.X > navbarMiddle - avatarWidth / 2f &&
                                       point.X < navbarMiddle + avatarWidth / 2f;
                            }
                            return false;
                        }

                        bool IsInsideLeftTopCorner(PointF point, SKRect rect)
                        {
                            return point.X < rect.Left + 100 * scale && point.Y < rect.Top + pxHotspotHeight;
                        }

                        bool IsInsideRightTopCorner(PointF point, SKRect rect)
                        {
                            return point.X > rect.Right - 100 * scale && point.Y < rect.Top + pxHotspotHeight;
                        }

                        bool avatarClicked = false;
                        bool weaponClicked = false;
                        bool consumed = false;

                        if (_doom.IsCapturingMouse) //playing
                        {
                            if (IsInsideLeftTopCorner(args.Event.Location, viewport))
                            {
                                if (!_calbackUi.Invoke(UiCommand.Reset))
                                {
                                    SetKeyStatus(EventType.KeyDown, DoomKey.Escape, _doom, currentTime);
                                }
                            }
                            else
                            if (IsInsideRightTopCorner(args.Event.Location, viewport))
                            {
                                //top-right corner
                                if (!_calbackUi.Invoke(UiCommand.Reset))
                                {
                                    SetKeyStatus(EventType.KeyDown, DoomKey.Tab, _doom, currentTime);
                                }
                            }
                            else
                            if (IsInsideLeftBottomCorner(args.Event.Location, viewport))
                            {
                                if (_calbackUi.Invoke(UiCommand.SelectWeapon))
                                {
                                    weaponClicked = true;
                                }
                            }
                            else
                            if (IsInsideAvatar(args.Event.Location, viewport))
                            {
                                //clicked avatar
                                if (!_calbackUi.Invoke(UiCommand.Reset))
                                {
                                    avatarClicked = true;
                                    SetKeyStatus(EventType.KeyDown, config.key_use.Keys[0], _doom, currentTime);
                                }
                            }
                            else
                            {
                                if (!_calbackUi.Invoke(UiCommand.Reset))
                                {
                                    _pressedFire = currentTime;
                                }
                            }
                            //if (!avatarClicked && !weaponClicked)
                            //{
                            //    //just clicked bottom UI bar
                            //    if (!_calbackUi.Invoke(UiCommand.Reset))
                            //    {
                            //        SetKeyStatus(EventType.KeyDown, DoomKey.Escape, _doom, currentTime);
                            //    }
                            //}

                        }
                        else
                        {
                            if (IsInsideLeftTopCorner(args.Event.Location, viewport))
                            {
                                if (!_calbackUi.Invoke(UiCommand.Reset))
                                {
                                    SetKeyStatus(EventType.KeyDown, DoomKey.Escape, _doom, currentTime);
                                }
                            }
                            else
                                SetKeyStatus(EventType.KeyDown, DoomKey.Enter, _doom, currentTime);
                        }
                    }
                }
            }

            _isPressed = false;
        }

        return true;
    }

    public void SelectWeapon(int number)
    {
        var currentTime = new EventTimestamp(_lastFrame);

        DoomKey weaponKey = number switch
        {
            2 => DoomKey.Num2,
            3 => DoomKey.Num3,
            4 => DoomKey.Num4,
            5 => DoomKey.Num5,
            6 => DoomKey.Num6,
            7 => DoomKey.Num7,
            _ => DoomKey.Unknown
        };

        if (weaponKey != DoomKey.Unknown)
            SetKeyStatus(EventType.KeyDown, weaponKey, _doom, currentTime);
    }


    /// <summary>
    /// Set the status of a key, Down or Up.
    /// </summary>
    /// <param name="type"></param>
    /// <param name="key"></param>
    /// <param name="doom"></param>
    public void SetKeyStatus(EventType type, MauiKey key, Doom doom, EventTimestamp currentTime)
    {
        SetKeyStatus(type, KeyToDoom(key), doom, currentTime);
    }

    /// <summary>
    /// Check if we need to simulate release for keys that were pressed by touch.
    /// </summary>
    /// <param name="doom"></param>
    /// <param name="currentTime"></param>
    public void AutoReleaseKeys(Doom doom, EventTimestamp currentTime)
    {
        AutoReleaseKey(DoomKey.Enter, doom, 50, currentTime);
        AutoReleaseKey(DoomKey.Escape, doom, 50, currentTime);

        if (AutoReleaseKey(DoomKey.Up, doom, 200, currentTime))
        {
            _moveUp = false;
        }
        if (AutoReleaseKey(DoomKey.Down, doom, 200, currentTime))
        {
            _moveDown = false;
        }

        /*
        if (AutoReleaseKey(DoomKey.Left, doom, 200, currentTime))
        {
            _moveLeft = false;
        }
        if (AutoReleaseKey(DoomKey.Right, doom, 200, currentTime))
        {
            _moveRight = false;
        }
        */

        AutoReleaseKey(_config.key_use.Keys[0], doom, 30, currentTime);

        AutoReleaseKey(DoomKey.Tab, doom, 100, currentTime);

        AutoReleaseKey(DoomKey.Num2, doom, 100, currentTime);
        AutoReleaseKey(DoomKey.Num3, doom, 100, currentTime);
        AutoReleaseKey(DoomKey.Num4, doom, 100, currentTime);
        AutoReleaseKey(DoomKey.Num5, doom, 100, currentTime);
        AutoReleaseKey(DoomKey.Num6, doom, 100, currentTime);
        AutoReleaseKey(DoomKey.Num7, doom, 100, currentTime);
    }

    /// <summary>
    /// When we simulate a key press with mobile touch, we need to release it automatically after a while.
    /// This checks for time elapsed after we pressed and if it's more than the time we want to keep it pressed releases the key.
    /// </summary>
    /// <param name="doomKey"></param>
    /// <param name="doom"></param>
    /// <param name="timeMsAfterPressed"></param>
    /// <returns></returns>
    public bool AutoReleaseKey(DoomKey doomKey, Doom doom, int timeMsAfterPressed, EventTimestamp now)
    {
        if (Pressed.TryGetValue(doomKey, out var pressed))
        {
            if (pressed.Expired(now, timeMsAfterPressed))
            {
                Trace.WriteLine($"[EVENT] {EventType.KeyUp} {doomKey} at {Pressed[doomKey].Frame}");
                Pressed[doomKey] = EventTimestamp.Empty;
                doom.PostEvent(new DoomEvent(EventType.KeyUp, doomKey));
                return true;
            }
        }
        return false;
    }


    private const int doubleTapMs = 1000;


    private bool _moveDown;
    private bool _moveUp;
    private readonly Func<UiCommand, bool> _calbackUi;
    private long _lastFrame;
    private Doom _doom;
    private bool _desktop;
    private long _lastDownTime;

    public class VirtualMouse
    {
        public PointF Position { get; set; }
    }
}

