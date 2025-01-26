using DrawnUi.Maui.Infrastructure;
using SkiaSharp;
using System.Runtime.InteropServices;

namespace ManagedDoom.Maui.Game
{
    public class ShadersHelper
    {
        private SKBitmap CreateIndexBitmap(int width, int height, byte[] screenData)
        {
            // width * height must match screenData.Length
            if (screenData.Length != width * height)
                throw new ArgumentException("screenData length does not match width*height.");

            // Create an 8-bit alpha-only bitmap
            var info = new SKImageInfo(width, height, SKColorType.Alpha8, SKAlphaType.Opaque);
            var indexBitmap = new SKBitmap(info);

            var pixelsPtr = indexBitmap.GetPixels();
            if (pixelsPtr != IntPtr.Zero)
            {
                Marshal.Copy(screenData, 0, pixelsPtr, screenData.Length);
            }

            return indexBitmap;
        }


        private SKRuntimeEffect? _compiledCode;
        private SKPaint _paint = new SKPaint();

        private void LoadShader()
        {
            var sksl = SkSl.LoadFromResources(@"Shaders\writedata.sksl");

            _compiledCode = SKRuntimeEffect.CreateShader(sksl, out var error);
            if (_compiledCode == null)
            {
                throw new Exception($"Shader compilation failed: {error}");
            }
        }

        public void ApplyShader(SKCanvas canvas, byte[] screenData, uint[] colors)
        {
            if (_compiledCode == null) LoadShader();

            if (_compiledCode != null)
            {
                var shaderBuilder = new SKRuntimeShaderBuilder(_compiledCode);

                //????
                int[] colorsInt = MemoryMarshal.Cast<uint, int>(colors).ToArray(); // One-time allocation
                Span<int> screenDataSpan = MemoryMarshal.Cast<byte, int>(screenData);
                int[] screenDataInt = GC.AllocateUninitializedArray<int>(screenDataSpan.Length);
                screenDataSpan.CopyTo(screenDataInt); // Copy only once, no repeated allocations

                shaderBuilder.Uniforms["colors"] = colorsInt;  // Already in int[], no extra allocation
                shaderBuilder.Uniforms["screenDataSize"] = screenData.Length;
                shaderBuilder.Uniforms["screenData"] = screenDataInt;  // No unnecessary `.ToArray()` calls
                var shader = shaderBuilder.Build();
                _paint.Shader = shader;

                // Draw full-screen rect?
                canvas.DrawRect(new SKRect(0, 0, screenData.Length, 1), _paint);
            }



        }

    }
}
