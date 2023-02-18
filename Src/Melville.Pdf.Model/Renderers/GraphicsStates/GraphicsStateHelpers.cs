using System.Numerics;

namespace Melville.Pdf.Model.Renderers.GraphicsStates;

public static class GraphicsStateHelpers
{
    /// <summary>
    /// Is the current line style  dashed.
    /// </summary>
    /// <param name="gs">The graphic state being queried.</param>
    /// <returns>True if the current line style is likely to be dashed,
    /// false if it is definitely solid.</returns>
    public static bool IsDashedStroke(this GraphicsState gs) =>
        gs.DashArray.Length > 0;

    /// <summary>
    /// In PDF zero line widths are one pixel wide.  This converts a raw width to the actual width.
    /// </summary>
    /// <param name="state">The graphic state being queried.</param>
    /// <returns>The width of the line to be drawn at the current time.</returns>
    public static double EffectiveLineWidth(this GraphicsState state)
    {
        if (state.LineWidth > double.Epsilon) return state.LineWidth;
        if (!Matrix3x2.Invert(state.TransformMatrix, out var invmat)) return 1;
        return invmat.M11;
    }

    /// <summary>
    /// The matrix that will transform the next text glyph to be rendered, accomodating
    /// all the current graphhic parameters.
    /// </summary>
    /// <param name="s">The graphic state being queried.</param>
    public static Matrix3x2 GlyphTransformMatrix(this GraphicsState s)
    {
        var tFs = (float)s.FontSize;
        var tHs = (float)s.HorizontalTextScale;
        var tRise = (float)s.TextRise;

        return new
            Matrix3x2(
                tFs * tHs, 0,
                0, tFs,
                0, tRise) * s.TextMatrix;
    }
}