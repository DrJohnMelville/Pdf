using System.Numerics;
using Melville.Pdf.Model.Renderers.Colors;

namespace Melville.Pdf.Model.Renderers.Patterns.ShaderPatterns;

internal abstract class PixelQueryFunctionalShader: IShaderWriter
{
    private readonly Matrix3x2 pixelsToPattern;
    protected IColorSpace ColorSpace { get; }
    private readonly RectInterval bboxInterval;
    protected uint BackgroundColor { get; }

    protected PixelQueryFunctionalShader(in CommonShaderValues values)
    {
        (pixelsToPattern, ColorSpace, bboxInterval, BackgroundColor) = values;
    }

    public unsafe void RenderBits(uint* bits, int width, int height)
    {
        uint* pos = bits; 
        for (int i = 0; i < height; i++)
        {
            for (int j = 0; j < width; j++)
            {
                uint desired = 0;
                desired = ColorForPixel(new Vector2(j, i));
                *pos++ = desired;
            }
        }
    }

    private uint ColorForPixel(in Vector2 pixel)
    {
        var patternVal = Vector2.Transform(pixel, pixelsToPattern);
        return bboxInterval.OutOfRange(patternVal) ? 
            0 : 
            GetColorFromShader(patternVal);
    }

    protected abstract uint GetColorFromShader(Vector2 patternVal);
}