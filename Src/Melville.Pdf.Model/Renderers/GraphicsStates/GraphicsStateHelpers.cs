using System.Numerics;

namespace Melville.Pdf.Model.Renderers.GraphicsStates;

public static class GraphicsStateHelpers
{
    public static bool IsDashedStroke(this GraphicsState gs) =>
        gs.DashArray.Length > 0;


    public static double EffectiveLineWidth(this GraphicsState state)
    {
        if (state.LineWidth > double.Epsilon) return state.LineWidth;
        if (!Matrix3x2.Invert(state.TransformMatrix, out var invmat)) return 1;
        return invmat.M11;
    }

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