using System;
using System.Threading.Tasks;
using Melville.Pdf.Model.Renderers.Bitmaps;
using Melville.Pdf.Model.Renderers.Colors;

namespace Melville.Pdf.Model.Renderers.Patterns.ShaderPatterns;

internal class ShaderBitmap : IPdfBitmap
{
    private readonly IShaderWriter source;
    public int Width { get; }
    public int Height { get; }

    public ShaderBitmap(IShaderWriter source, int width, int height)
    {
        this.source = source;
        Width = width;
        Height = height;
    }

    public bool DeclaredWithInterpolation => true;
    public unsafe ValueTask RenderPbgraAsync(Memory<DeviceColor> buffer)
    {
        var ptr = buffer.GetPointer();
        source.RenderBits((uint*)ptr, Width, Height);
        return ValueTask.CompletedTask;
    }
}