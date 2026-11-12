using System;
using System.Numerics;
using Melville.Pdf.Model.Renderers.Colors;

namespace Melville.Pdf.Model.Renderers.Patterns.ShaderPatterns;

internal abstract class PixelQueryFunctionalShader: IShaderWriter
{
    private readonly Matrix3x2 pixelsToPattern;
    protected IColorSpace ColorSpace { get; }
    private readonly RectInterval bboxInterval;
    protected DeviceColor BackgroundColor { get; }

    protected PixelQueryFunctionalShader(in CommonShaderValues values)
    {
        (pixelsToPattern, ColorSpace, bboxInterval, BackgroundColor) = values;
    }

    public unsafe void RenderBits(Span<DeviceColor> bits, int width, int height)
    {
        int pos = 0;
        for (int i = 0; i < height; i++)
        {
            for (int j = 0; j < width; j++)
            {
                bits[pos++] = ColorForPixel(new Vector2(j, i)); ;
            }
        }
    }

    private DeviceColor ColorForPixel(in Vector2 pixel)
    {
        var patternVal = Vector2.Transform(pixel, pixelsToPattern);
        return bboxInterval.OutOfRange(patternVal) ? 
            DeviceColor.Invisible : 
            GetColorFromShader(patternVal);
    }

    protected abstract DeviceColor GetColorFromShader(Vector2 patternVal);
}