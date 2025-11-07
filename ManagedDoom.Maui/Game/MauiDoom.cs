using AppoMobi.Maui.Gestures;
using DrawnUi.Draw;
using DrawnUi.Gaming;
using Plugin.Maui.Audio;

namespace ManagedDoom.Maui.Game;

public class MauiDoom : MauiGame
{
    private MauiMusic _music;
    private MauiSound _sound;
    private MauiVideo _video;
    private MauiUserInput _input;

    private CommandLineArgs args;
    private Config _config;
    private GameContent _content;
    private Doom _doom;

    private int fpsScale;
    private int frameCount;
    private bool _initialized;

    protected WeaponsView _uiSelectWeapon; //for selecting weaps
    protected Keypad? _uiHud; //will show while playing if enabled in MauiProgram.cs

    public MauiDoom()
    {
        try
        {
            HorizontalOptions = LayoutOptions.Fill;
            VerticalOptions = LayoutOptions.Fill;

            PlatformHelpers.ConfigUtilities = new ConfigUtilities();

            var args = new CommandLineArgs(new string[]
            {

            });

            this.args = args;

        }
        catch (Exception e)
        {
            Super.DisplayException(this, e);
        }
    }


    /// <summary>
    /// Normally all input goes to doom, but we want to intercept some
    /// keys here so show our own UI for selecting weapons etc.
    /// Will be invoked by MauiUserInput in that case
    /// </summary>    
    /// <param name="command"></param>
    /// <returns>was consumed</returns>
    bool OnUiCommand(UiCommand command)
    {
        switch (command)
        {
        case UiCommand.SelectWeapon:
        {
            if (!_uiSelectWeapon.IsVisible)
            {
                _uiSelectWeapon.IsVisible = true;
                return true;
            }
        }
        break;

        case UiCommand.Reset:
        default:
        if (_uiSelectWeapon.IsVisible)
        {
            _uiSelectWeapon.IsVisible = false;
            return true;
        }
        break;
        }

        return false;
    }

 
    /// <summary>
    /// Paint the game, invoked every frame
    /// </summary>
    protected override void Paint(DrawingContext ctx)
    {
        // base.Paint(ctx); - will not draw background color/gradient in this case

        frameCount++;

        //we draw the game every frame but sending the frameFrac 0 or 1 to interpolate updates
        //var frameFrac = Fixed.FromInt(frameCount % fpsScale + 1) / fpsScale;
        var frameFrac = Fixed.FromInt(1);

        //we update the game every 2 frames
        if (frameCount % fpsScale == 0)
        {
            _input.Update(_doom, new EventTimestamp(frameCount));

            if (_doom.Update() == UpdateResult.Completed)
            {
                //exit game, we just reload to splash
                Reload();
                return;
            }

            frameFrac /= 2;
        }

        //render doom
        _video.Render(ctx.Context.Canvas, ctx.Destination, _doom, frameFrac);

        //render custom ui
        var drawnChildrenCount = DrawViews(ctx);//GetDoomScale(DrawingRect, scale));
    }



    /// <summary>
    /// Setup everything
    /// </summary>
    /// <param name="context"></param>
    /// <param name="destination"></param>
    /// <param name="scale"></param>
    void Init(DrawingContext context)
    {
        frameCount = -1;

        _config = ConfigUtilities.GetConfig();
        _content = new GameContent(args);

        _config.video_screenwidth = Math.Clamp(_config.video_screenwidth, 320, 3200);
        _config.video_screenheight = Math.Clamp(_config.video_screenheight, 200, 2000);

        _config.video_fpsscale = Math.Clamp(_config.video_fpsscale, 1, 100); // 2

        var targetFps = 35 * _config.video_fpsscale;  //todo apply to gameloop?

        _video = new MauiVideo(_config, _content, context.Context);

        if (!args.nosound.Present && MauiProgram.UseSound)
        {
            var audioDevice = AudioManager.Current;

            _sound = new MauiSound(_config, _content, audioDevice);

            //todo implement music
            //if (!args.nomusic.Present)
            //{
            //    _music = ConfigUtilities.GetMusicInstance(_config, _content, audioDevice);
            //}
        }

        _input = new MauiUserInput(_config, !args.nomouse.Present, OnUiCommand);

        _doom = new Doom(args, _config, _content, _video, _sound, _music, _input);

        _input.Attach(_doom);

        _uiSelectWeapon = new WeaponsView(_input.SelectWeapon);
        _uiSelectWeapon.IsVisible = false;
        this.AddSubView(_uiSelectWeapon);

        if (MauiProgram.ShowKeys)
        {
            _uiHud = new Keypad(this);
            SetHudVisibility();
            this.AddSubView(_uiHud);
        }

        fpsScale = args.timedemo.Present ? 1 : _config.video_fpsscale; //2
        frameCount = -1;
    }

    void Reload()
    {
        StopLoop();
        _initialized = false;
    }

    void SetHudVisibility()
    {
        if (_uiHud != null)
        {
            var value = _doom.IsPlaying;
            if (_uiHud.IsVisible != value)
            {
                _uiHud.IsVisible = value;
            }
        }
    }

    /// <summary>
    /// Initialise the game when ready to draw, one time
    /// </summary>
    protected override void Draw(DrawingContext context)
    {
        if (!_initialized)
        {
            _initialized = true;
            Init(context);
            StartLoop();
        }

        SetHudVisibility();

        base.Draw(context);
    }

    public override void OnDisposing()
    {
        base.OnDisposing();

        _input?.Dispose();

        OnClose();
    }

    void OnClose()
    {
        _input?.Dispose();
        _input = null;

        _music?.Dispose();
        _music = null;

        _sound?.Dispose();
        _sound = null;

        //    audioDevice?.Dispose();
        //    audioDevice = null;

        if (_video is IDisposable disposeVideo)
            disposeVideo.Dispose();
        _video = null;

        _config?.Save(PlatformHelpers.ConfigUtilities.GetConfigPath());
    }

    #region INPUT

    /// <summary>
    /// Pass to doom
    /// </summary>
    /// <param name="key"></param>
    public override void OnKeyDown(MauiKey key)
    {
        var currentTime = new EventTimestamp(frameCount);
        _input?.SetKeyStatus(EventType.KeyDown, key, _doom, currentTime);
    }

    /// <summary>
    /// Pass to doom
    /// </summary>
    /// <param name="key"></param>
    public override void OnKeyUp(MauiKey key)
    {
        var currentTime = new EventTimestamp(frameCount);
        _input.SetKeyStatus(EventType.KeyUp, key, _doom, currentTime);
    }

    public override ISkiaGestureListener ProcessGestures(SkiaGesturesParameters args, GestureEventProcessingInfo apply)
    {

        //send to children first..
        bool passedToChildren = false;
        ISkiaGestureListener PassToChildren()
        {
            passedToChildren = true;
            return base.ProcessGestures(args, apply);
        }

         ISkiaGestureListener consumed = PassToChildren();
        if (consumed != null && args.Type != TouchActionResult.Up)
        {
            return consumed;
        }

        //now can pass to input implementation
        var handled = _input.ProcessGestures(args, apply,
            RenderingScale, DrawingRect, _video.Viewport, frameCount, _config);
        if (handled)
            return this;

        return consumed;
    }

    public enum UiCommand
    {
        Reset,
        SelectWeapon
    }

    #endregion
}