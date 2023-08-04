using System.Numerics;

namespace Melville.Pdf.Model.Renderers.GraphicsStates;

/// <summary>
/// Static class that holds a few extension methods on the GraphicsState object
/// </summary>
public static class GraphicsStateHelpers
{
    /// <summary>
    /// Is the current line style  dashed.
    /// </summary>
    /// <param name="gs">The graphic state being queried.</param>
    /// <returns>True if the current line style is likely to be dashed,
    /// false if it is definitely solid.</returns>
    public static bool IsDashedStroke(this GraphicsState gs) =>
        gs.DashArray.Count > 0;

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

    /// <summary>
    /// Compute a transform that will revert the current matrix bac; to the current matrix.
    /// </summary>
    /// <param name="gs"></param>
    /// <returns></returns>
    public static Matrix3x2 RevertToPixelsMatrix(this GraphicsState gs)
    {
        Matrix3x2.Invert(gs.InitialTransformMatrix, out var invInitial);
        Matrix3x2.Invert(gs.TransformMatrix * invInitial, out var ret);
        return ret;
    }

}