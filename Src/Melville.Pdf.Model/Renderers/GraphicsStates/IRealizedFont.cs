using System;
using System.Numerics;
using Melville.Pdf.LowLevel.Model.ContentStreams;

namespace Melville.Pdf.Model.Renderers.GraphicsStates;

public interface IFontWriteTarget<T>
{
    Matrix3x2 CharacterPositionMatrix();
    void RenderCurrentString(T currentString, bool stroke, bool fill, bool clip);
}

public interface IFontWriteOperation
{
    (double width, double height) AddGlyphToCurrentString(byte b);
    void RenderCurrentString(bool stroke, bool fill, bool clip);
}
public interface IRealizedFont
{
    IFontWriteOperation BeginFontWrite();
}

public sealed class NullRealizedFont: IFontWriteOperation, IRealizedFont
{
    public static readonly NullRealizedFont Instance = new();

    private NullRealizedFont() { }
    public (double width, double height) AddGlyphToCurrentString(byte b) => (0.0, 0.0);

    public void RenderCurrentString(bool stroke, bool fill, bool clip)
    {
    }

    public IFontWriteOperation BeginFontWrite() => this;
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