using System;
using System.Numerics;
using System.Threading.Tasks;
using Melville.INPC;
using Melville.Pdf.LowLevel.Model.ContentStreams;
using Melville.Pdf.Model.Renderers.FontRenderings.Type3;

namespace Melville.Pdf.Model.Renderers.FontRenderings;

public interface IFontWriteOperation
{
    ValueTask<double> AddGlyphToCurrentString(uint glyph, Matrix3x2 textMatrix);
    void RenderCurrentString(bool stroke, bool fill, bool clip);
}
public interface IRealizedFont
{
    (uint character, uint glyph, int bytesConsumed) GetNextGlyph(in ReadOnlySpan<byte> input);
    double CharacterWidth(uint character, double defaultWidth);
    IFontWriteOperation BeginFontWrite(IFontTarget target);
}

[StaticSingleton]
public sealed partial class NullRealizedFont: IFontWriteOperation, IRealizedFont
{
    public (uint character, uint glyph, int bytesConsumed) GetNextGlyph(in ReadOnlySpan<byte> input) => (0, 0, 1);

    public ValueTask<double> AddGlyphToCurrentString(uint glyph, Matrix3x2 textMatrix) => new(0.0);

    public double CharacterWidth(uint character, double defaultWidth) => defaultWidth;

    public void RenderCurrentString(bool stroke, bool fill, bool clip)
    {
    }

    public IFontWriteOperation BeginFontWrite(IFontTarget target) => this;
}

public static class FontWriteOperationsImpl
{
    public static void RenderCurrentString(this IFontWriteOperation op, TextRendering rendering)
    {
        switch (rendering)
        {
            case TextRendering.Fill:
                op.RenderCurrentString(false, true, false);
                break;
            case TextRendering.Stroke:
                op.RenderCurrentString(true, false, false);
                break;
            case TextRendering.FillAndStroke:
                op.RenderCurrentString(true, true, false);
                break;
            case TextRendering.Invisible:
                op.RenderCurrentString(false, false, false);
                break;
            case TextRendering.FillAndClip:
                op.RenderCurrentString(false, true, true);
                break;
            case TextRendering.StrokeAndClip:
                op.RenderCurrentString(true, false, true);
                break;
            case TextRendering.FillStrokeAndClip:
                op.RenderCurrentString(true, true, true);
                break;
            case TextRendering.Clip:
                op.RenderCurrentString(false, false, true);
                break;
        }
    }
}