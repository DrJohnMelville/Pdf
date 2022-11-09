namespace Melville.Pdf.Model.Renderers.OptionalContents;

public static class RenderTargetExtension
{
    public static void ConditionalPaintPath(
        this IDrawTarget target, bool show, bool stroke, bool fill, bool evenOddFillRule) =>
        target.PaintPath(show && stroke, fill && stroke, evenOddFillRule);
}