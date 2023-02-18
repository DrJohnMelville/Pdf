using System.Threading.Tasks;
using Melville.Pdf.Model.Renderers.Bitmaps;

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
    public unsafe ValueTask RenderPbgra(byte* buffer)
    {
        source.RenderBits((uint*)buffer, Width, Height);
        return ValueTask.CompletedTask;
    }
}