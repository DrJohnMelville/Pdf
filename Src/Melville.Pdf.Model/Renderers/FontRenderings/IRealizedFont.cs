using System;
using System.Numerics;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.ContentStreams;
using Melville.Pdf.Model.Renderers.FontRenderings.Type3;

namespace Melville.Pdf.Model.Renderers.FontRenderings;

public interface IFontWriteOperation: IDisposable
{
    ValueTask<double> AddGlyphToCurrentString(uint glyph, Matrix3x2 textMatrix);
    void RenderCurrentString(bool stroke, bool fill, bool clip);
}
public interface IRealizedFont
{
    (uint character, uint glyph, int bytesConsumed) GetNextGlyph(in ReadOnlySpan<byte> input);
    double CharacterWidth(uint character, double defaultWidth);
    IFontWriteOperation BeginFontWrite(IFontTarget target);
    IFontWriteOperation BeginFontWriteWithoutTakingMutex(IFontTarget target);
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