using ManagedDoom;
using ManagedDoom.Video;
using SkiaSharp;
using System.Runtime.InteropServices;

namespace UnoDoom.Game;

public class UnoVideo : IVideo, IDisposable
{
    private Renderer renderer;
    private byte[] textureData;
    private int _textureWidth;
    private int _textureHeight;
    private SKPaint _paint;
    private SKBitmap _texture;
    private SKRuntimeEffect? _shader;
    private SKRuntimeShaderBuilder? _gpuBuilder;

    public SKRect Viewport { get; private set; }

    public UnoVideo(Config config, GameContent content)
    {
        try
        {
            Console.Write("Initialize video: ");

            renderer = new Renderer(config, content);

            if (config.video_highresolution)
            {
                _textureWidth = 400;
                _textureHeight = 640;
            }
            else
            {
                _textureWidth = 200;
                _textureHeight = 320;
            }

            _texture = new SKBitmap(_textureWidth, _textureHeight, SKColorType.Rgba8888, SKAlphaType.Premul);
            textureData = new byte[4 * renderer.Width * renderer.Height];

            _paint = new SKPaint()
            {
                FilterQuality = SKFilterQuality.High,
                IsAntialias = false
            };

            Console.WriteLine("OK");
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
        // Render game to texture
        RenderToTexture(doom, frameFrac);

        // Calculate viewport with aspect ratio
        float centerX = destination.MidX;
        float centerY = destination.MidY;

        SKRect viewport;
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

        // Center the viewport
        float fittedX = destination.MidX - (fittedWidth / 2);
        float fittedY = destination.MidY - (fittedHeight / 2);

        viewport = new SKRect(
            fittedX,
            fittedY,
            fittedX + fittedWidth,
            fittedY + fittedHeight
        );

        Viewport = viewport;

        canvas.Save();
        canvas.Translate(centerX, centerY);
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

    private void CreateShader()
    {
        string shaderCode = @"
            uniform float2 iOffset;
            uniform float2 iResolution;
            uniform float2 iImageResolution;
            uniform shader iImage1;

            half4 main(float2 fragCoord) {
                float2 adjustedCoord = fragCoord - iOffset;
                float2 uv = adjustedCoord / iResolution;
                float2 imageCoord = uv * iImageResolution;
                return iImage1.eval(imageCoord);
            }";

        var effect = SKRuntimeEffect.CreateShader(shaderCode, out string? errorMessage);
        if (effect == null || !string.IsNullOrEmpty(errorMessage))
        {
            throw new Exception($"Shader compilation error: {errorMessage}");
        }

        _shader = effect;
        _gpuBuilder = new SKRuntimeShaderBuilder(_shader);
    }

    private unsafe void RenderToTexture(Doom doom, Fixed frameFrac)
    {
        if (doom.Wiping)
        {
            RenderWipe(doom);
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

    private void RenderWipe(Doom doom)
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

        Parallel.For(0, height, y =>
        {
            uint* rowPtr = (uint*)(basePtr + y * rowBytes);
            int screenRowOffset = y * width;
            for (int x = 0; x < width; x++)
            {
                byte indexVal = screenData[screenRowOffset + x];
                rowPtr[x] = palette[indexVal];
            }
        });
    }

    public void InitializeWipe()
    {
        renderer.InitializeWipe();
    }

    public void Render(Doom doom, Fixed frameFrac)
    {
        // This is the simple render method without canvas - not used in Uno implementation
        // We use the SKCanvas version instead
    }

    public bool HasFocus()
    {
        return true;
    }

    public void Dispose()
    {
        _texture?.Dispose();
        _paint?.Dispose();
        _shader?.Dispose();
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
