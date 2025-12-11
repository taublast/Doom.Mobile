using ManagedDoom;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using SkiaSharp;
using SkiaSharp.Views.Windows;
using System.Diagnostics;
#if WINDOWS || __MACCATALYST__ || __MACOS__
using Microsoft.UI.Xaml.Input;
using Windows.System;
#endif

namespace UnoDoom.Game;

public partial class UnoDoomGame : UserControl
{
    private UnoVideo? _video;
    private UnoSound? _sound;
    private UnoMusic? _music;
    private UnoUserInput? _input;

    private CommandLineArgs? args;
    private Config? _config;
    private GameContent? _content;
    private Doom? _doom;

    private int fpsScale;
    private int frameCount;
    private bool _initialized;

    private DispatcherTimer? _gameTimer;
    private SKXamlCanvas _canvas;

    public UnoDoomGame()
    {
        this.InitializeComponent();
        
        HorizontalAlignment = HorizontalAlignment.Stretch;
        VerticalAlignment = VerticalAlignment.Stretch;

        _canvas = new SKXamlCanvas();
        _canvas.PaintSurface += OnPaintSurface;
        _canvas.HorizontalAlignment = HorizontalAlignment.Stretch;
        _canvas.VerticalAlignment = VerticalAlignment.Stretch;
        
        Content = _canvas;

        this.Loaded += UnoDoomGame_Loaded;
        this.Unloaded += UnoDoomGame_Unloaded;
        this.KeyDown += UnoDoomGame_KeyDown;
        this.KeyUp += UnoDoomGame_KeyUp;
    }

    private async void UnoDoomGame_Loaded(object sender, RoutedEventArgs e)
    {
        await InitializeGameAsync();
        StartGameLoop();
    }

    private void UnoDoomGame_Unloaded(object sender, RoutedEventArgs e)
    {
        StopGameLoop();
        CleanupGame();
    }

    private async Task InitializeGameAsync()
    {
        if (_initialized)
            return;

        try
        {
#if __WASM__
            // For WebAssembly, prepare assets first
            await ConfigUtilities.PrepareAssetsAsync();
#endif

            PlatformHelpers.ConfigUtilities = new ConfigUtilities();

            args = new CommandLineArgs(new string[] { });
            var configUtilities = new ConfigUtilities();
            _config = configUtilities.GetConfig();
            _content = new GameContent(args);

            _config.video_screenwidth = Math.Clamp(_config.video_screenwidth, 320, 3200);
            _config.video_screenheight = Math.Clamp(_config.video_screenheight, 200, 2000);
            _config.video_fpsscale = Math.Clamp(_config.video_fpsscale, 1, 100);

            _video = new UnoVideo(_config, _content);
            _sound = new UnoSound(_config, _content);
            _music = new UnoMusic(_config, _content);
            _input = new UnoUserInput(_config, !args.nomouse.Present);

            _doom = new Doom(args, _config, _content, _video, _sound, _music, _input);

            fpsScale = args.timedemo.Present ? 1 : _config.video_fpsscale;
            frameCount = -1;

            _initialized = true;
            Console.WriteLine("Game initialized successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to initialize game: {ex.Message}");
            Debug.WriteLine($"Game initialization error: {ex}");
        }
    }

    private void StartGameLoop()
    {
        _gameTimer = new DispatcherTimer();
        _gameTimer.Interval = TimeSpan.FromMilliseconds(1000.0 / 60.0); // 60 FPS target
        _gameTimer.Tick += GameTimer_Tick;
        _gameTimer.Start();
    }

    private void StopGameLoop()
    {
        if (_gameTimer != null)
        {
            _gameTimer.Stop();
            _gameTimer.Tick -= GameTimer_Tick;
            _gameTimer = null;
        }
    }

    private void GameTimer_Tick(object? sender, object e)
    {
        _canvas?.Invalidate();
    }

    private void OnPaintSurface(object sender, SKPaintSurfaceEventArgs e)
    {
        if (!_initialized || _doom == null)
            return;

        var canvas = e.Surface.Canvas;
        canvas.Clear(SKColors.Black);

        var destination = new SKRect(0, 0, e.Info.Width, e.Info.Height);

        frameCount++;

        var frameFrac = Fixed.FromInt(1);

        // Update game every N frames based on fpsScale
        if (frameCount % fpsScale == 0)
        {
            _input?.Update(_doom!, new EventTimestamp(frameCount));

            if (_doom!.Update() == UpdateResult.Completed)
            {
                // Game completed - could reload or show menu
                return;
            }

            frameFrac /= 2;
        }

        // Render the game
        _video?.Render(canvas, destination, _doom!, frameFrac);
    }

    private void UnoDoomGame_KeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (_doom == null || _input == null || !_initialized)
            return;

        var doomKey = UnoUserInput.VirtualKeyToDoom(e.Key);
        if (doomKey != DoomKey.Unknown)
        {
            _input.SetKeyStatus(EventType.KeyDown, doomKey, _doom, new EventTimestamp(frameCount));
            e.Handled = true;
        }
    }

    private void UnoDoomGame_KeyUp(object sender, KeyRoutedEventArgs e)
    {
        if (_doom == null || _input == null || !_initialized)
            return;

        var doomKey = UnoUserInput.VirtualKeyToDoom(e.Key);
        if (doomKey != DoomKey.Unknown)
        {
            _input.SetKeyStatus(EventType.KeyUp, doomKey, _doom, new EventTimestamp(frameCount));
            e.Handled = true;
        }
    }

    private void CleanupGame()
    {
        _doom = null;
        _video?.Dispose();
        _sound?.Dispose();
        _music?.Dispose();
        _input?.Dispose();
        _content = null;
        _initialized = false;
    }
}
