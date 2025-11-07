using DrawnUi.Draw;
using ManagedDoom.Video;
using SkiaSharp;
using System.Runtime.InteropServices;
using DrawnUi.Infrastructure;
using SKCanvas = SkiaSharp.SKCanvas;
using SKFilterQuality = SkiaSharp.SKFilterQuality;
using SKPaint = SkiaSharp.SKPaint;
using SKRect = SkiaSharp.SKRect;

namespace ManagedDoom.Maui.Game;

public class MauiVideo : IVideo, IDisposable
{
    private Renderer renderer;

    private byte[] textureData;

    private int skiaWindowWidth;
    private int skiaWindowHeight;
    private readonly float _scale;
    private int _textureWidth;
    private int _textureHeight;
    private readonly SKPaint _paint;
    private SKBitmap _texture;

    public MauiVideo(
        Config config,
        GameContent content,
        SkiaDrawingContext context)
    {
        try
        {
            Console.Write("Initialize video: ");

            renderer = new Renderer(config, content);

            if (config.video_highresolution)
            {
                _textureWidth = 400;//silk had 512;
                _textureHeight = 640;//silk had 1024;
            }
            else
            {
                _textureWidth = 200;// silk had 256;
                _textureHeight = 320;// silk had 512;
            }
            _texture = new SKBitmap(_textureWidth, _textureHeight, SKColorType.Rgba8888, SKAlphaType.Premul);

            textureData = new byte[4 * renderer.Width * renderer.Height];

            _paint = new SKPaint()
            {
                FilterQuality = SKFilterQuality.None,
                IsAntialias = false
            };

            if (!MauiProgram.IsMobile)
            {
                _paint.FilterQuality = SKFilterQuality.High;
            }
        }
        catch (Exception e)
        {
            Console.WriteLine("Failed");
            Dispose();
            throw;
        }
    }

    public void Render(SKCanvas canvas, SKRect destination, Doom doom, Fixed frameFrac)
    {
        if (DeviceInfo.Platform == DevicePlatform.iOS && DeviceInfo.DeviceType == DeviceType.Physical)
        {
            DrawScreen.Optimize = false;
            Render(doom, frameFrac);

            IntPtr pixPtr = _texture.GetPixels();
            if (pixPtr != IntPtr.Zero)
            {
                Marshal.Copy(textureData, 0, pixPtr, textureData.Length);
            }
        }
        else
        {
            //use multi-threading
            RenderUnsafer(doom, frameFrac); // _texture prepared inside
        }

        SKRect viewport;
        float centerX = destination.MidX;
        float centerY = destination.MidY;

        if (MauiProgram.KeepAspectRatio)
        {
            // DOOM aspect ratio
            float textureWidth = _textureWidth;
            float textureHeight = _textureHeight;
            float textureAspect = textureWidth / textureHeight;

            float screenWidth = destination.Width;
            float screenHeight = destination.Height;
            var screenAspect = screenHeight / screenWidth;

            float scaleFactor, fittedHeight, fittedWidth;

            if (textureAspect < screenAspect)
            {
                // Fit to width
                float asp = textureHeight / textureWidth;
                scaleFactor = screenWidth / textureWidth;
                fittedWidth = textureWidth * scaleFactor / asp;
                fittedHeight = textureHeight * scaleFactor / asp;
            }
            else
            {
                // Fit to height
                scaleFactor = screenHeight / textureHeight;
                fittedWidth = textureWidth * scaleFactor / textureAspect;
                fittedHeight = textureHeight * scaleFactor / textureAspect;
            }

            // center
            float fittedX = destination.MidX - (fittedWidth / 2);
            float fittedY = destination.MidY - (fittedHeight / 2);

            viewport = new SKRect(
                fittedX,
                fittedY,
                fittedX + fittedWidth,
                fittedY + fittedHeight
            );

        }
        else
        {
            float adjustedX = destination.Left + (destination.Width - destination.Height) / 2;
            float adjustedY = destination.Top + (destination.Height - destination.Width) / 2;

            viewport = new SKRect(
                adjustedX,
                adjustedY,
                adjustedX + destination.Height,
                adjustedY + destination.Width);
        }

        _viewportClipped = SkiaSvg.CalculateDisplayRect(destination, viewport.Height, viewport.Width,
            DrawImageAlignment.Center, DrawImageAlignment.Center);

        canvas.Save();

        canvas.Translate(centerX, centerY);

        //mobile
        //canvas.Scale(1, -1);

        canvas.RotateDegrees(-90);
        canvas.Scale(-1, 1);

        canvas.Translate(-centerX, -centerY);

        if (_shader == null)
        {
            CreateShader();
        }
        _gpuBuilder.Children.Add("iImage1", new(_texture.ToShader()));

        SKSize iResolution = new(viewport.Width, viewport.Height);
        SKSize iImageResolution = new(_texture.Width, _texture.Height);

        _gpuBuilder.Uniforms["iOffset"] = new[] { viewport.Left, viewport.Top };
        _gpuBuilder.Uniforms["iResolution"] = new[] { iResolution.Width, iResolution.Height };
        _gpuBuilder.Uniforms["iImageResolution"] = new[] { iImageResolution.Width, iImageResolution.Height };

        _paint.Shader = _gpuBuilder.Build();
        canvas.DrawRect(viewport, _paint);

        canvas.Restore();
    }

    public void RenderUnsafer(Doom doom, Fixed frameFrac)
    {
        if (doom.Wiping)
        {
            RenderWipeUnsafe(doom);
            return;
        }

        renderer.RenderDoom(doom, frameFrac);
        renderer.RenderMenu(doom);

        var palette = renderer.palette;

        var colors = palette[0];

        if (doom.Game.World != null)
        {
            if (doom.State == DoomState.Game &&
                doom.Game.State == GameState.Level)
            {
                colors = palette[Renderer.GetPaletteNumber(doom.Game.World.ConsolePlayer)];
            }
            else if (doom.State == DoomState.Opening &&
                     doom.Opening.State == OpeningSequenceState.Demo &&
                     doom.Opening.DemoGame.State == GameState.Level)
            {
                colors = palette[Renderer.GetPaletteNumber(doom.Opening.DemoGame.World.ConsolePlayer)];
            }
            else if (doom.State == DoomState.DemoPlayback &&
                     doom.DemoPlayback.Game.State == GameState.Level)
            {
                colors = palette[Renderer.GetPaletteNumber(doom.DemoPlayback.Game.World.ConsolePlayer)];
            }
        }

        WritePaletteDirectParallel(_texture, renderer.Screen.Data, colors);
    }

    public void RenderWipeUnsafe(Doom doom)
    {
        renderer.RenderDoom(doom, Fixed.One);

        var wipe = doom.WipeEffect;
        var scale = renderer.Screen.Width / 320;

        for (var i = 0; i < renderer.wipeBandCount - 1; i++)
        {
            var x1 = renderer.wipeBandWidth * i;
            var x2 = x1 + renderer.wipeBandWidth;
            var y1 = Math.Max(scale * wipe.Y[i], 0);
            var y2 = Math.Max(scale * wipe.Y[i + 1], 0);
            var dy = (float)(y2 - y1) / renderer.wipeBandWidth;
            for (var x = x1; x < x2; x++)
            {
                var y = (int)MathF.Round(y1 + dy * ((x - x1) / 2 * 2));
                var copyLength = renderer.Screen.Height - y;
                if (copyLength > 0)
                {
                    var srcPos = renderer.Screen.Height * x;
                    var dstPos = renderer.Screen.Height * x + y;
                    Array.Copy(renderer.ScreenSnapshot, srcPos, renderer.Screen.Data, dstPos, copyLength);
                }
            }
        }

        renderer.RenderMenu(doom);

        WritePaletteDirectParallel(_texture, renderer.Screen.Data, renderer.palette[0]);
    }

    public void Render(Doom doom, Fixed frameFrac)
    {
        renderer.Render(doom, textureData, frameFrac);
    }


    private unsafe void WritePaletteDirect(SKBitmap bitmap, byte[] screenData, uint[] palette)
    {
        // Grab the info so we know width, height, rowBytes, etc.
        var info = bitmap.Info;
        int width = info.Width;
        int height = info.Height;
        int rowBytes = info.RowBytes; // Could be >= width * 4

        // Get pointer to the first byte of the bitmap data
        IntPtr ptr = bitmap.GetPixels();
        if (ptr == IntPtr.Zero) return; // In case something’s invalid

        // Cast to a byte pointer so we can do row offsets
        byte* basePtr = (byte*)ptr;

        // We'll write row-by-row:
        for (int y = 0; y < height; y++)
        {
            // Row start for row y
            uint* rowPtr = (uint*)(basePtr + y * rowBytes);

            // Each row has 'width' pixels
            int screenRowOffset = y * width;
            for (int x = 0; x < width; x++)
            {
                byte indexVal = screenData[screenRowOffset + x];
                rowPtr[x] = palette[indexVal]; // Expand from 8-bit index to 32-bit color
            }
        }
    }

    private bool dev = false;
    //+2fps
    private unsafe void WritePaletteDirectParallel(SKBitmap bitmap, byte[] screenData, uint[] palette)
    {
        var info = bitmap.Info;
        int width = info.Width;
        int height = info.Height;
        int rowBytes = info.RowBytes;

        IntPtr ptr = bitmap.GetPixels();
        if (ptr == IntPtr.Zero)
            return;

        byte* basePtr = (byte*)ptr;

        // Parallelize over rows.
        Parallel.For(0, height, y =>
        {
            uint* rowPtr = (uint*)(basePtr + y * rowBytes);
            int screenRowOffset = y * width;

            for (int x = 0; x < width; x++)
            {
                // Expand from 8-bit index to 32-bit color
                byte indexVal = screenData[screenRowOffset + x];
                rowPtr[x] = palette[indexVal];
            }
        });
    }


    #region DOOM RENDERER

    private SKRuntimeEffect _gpuEffect;
    private SKRuntimeShaderBuilder _gpuBuilder;
    private SKPaint _gpuPaint;
    private SKBitmap _indexBitmap;
    private SKRuntimeEffect _shader;
    private SKRect _viewportClipped;

    public SKRect Viewport
    {
        get => _viewportClipped;
    }

    private void CreateShader()
    {
        var sksl = SkSl.LoadFromResources(@"Shaders\blit.sksl");
        _shader = SkSl.Compile(sksl, "blit");

        _gpuBuilder = new SKRuntimeShaderBuilder(_shader);
    }


    private void InitializeGpuShader()
    {
        var sksl = SkSl.LoadFromResources(@"Shaders\writedata.sksl");
        _gpuEffect = SkSl.Compile(sksl, "writedata");

        // 2) Create a builder and paint
        _gpuBuilder = new SKRuntimeShaderBuilder(_gpuEffect);
        _gpuPaint = new SKPaint();

        // 3) Create an 8-bit indexBitmap that matches DOOM's Screen size
        //    For instance, if your drawScreen is 320x200 (or 640x400).
        //    We'll create it once if the size never changes.
        var indexInfo = new SKImageInfo(renderer.Screen.Width,
            renderer.Screen.Height,
            SKColorType.Alpha8,   // or Gray8
            SKAlphaType.Opaque);
        _indexBitmap = new SKBitmap(indexInfo);
    }

    #endregion

    public void InitializeWipe()
    {
        renderer.InitializeWipe();
    }

    public bool HasFocus()
    {
        return true;
    }

    public void Dispose()
    {
        _texture?.Dispose();
        _texture = null;
        _paint?.Dispose();
    }

    public int WipeBandCount => renderer.WipeBandCount;
    public int WipeHeight => renderer.WipeHeight;
    public int MaxWindowSize => renderer.MaxWindowSize;

    public int WindowSize
    {
        get => renderer.WindowSize;
        set => renderer.WindowSize = value;
    }

    public bool DisplayMessage
    {
        get => renderer.DisplayMessage;
        set => renderer.DisplayMessage = value;
    }

    public int MaxGammaCorrectionLevel => renderer.MaxGammaCorrectionLevel;

    public int GammaCorrectionLevel
    {
        get => renderer.GammaCorrectionLevel;
        set => renderer.GammaCorrectionLevel = value;
    }
}