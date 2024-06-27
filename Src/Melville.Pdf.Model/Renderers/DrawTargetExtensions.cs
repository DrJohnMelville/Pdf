using System.Numerics;

namespace Melville.Pdf.Model.Renderers;

public static class DrawTargetExtensions
{
    /// <summary>
    /// Move the current point to a position.
    /// </summary>
    /// <param name="x">Horizontal coordinate to move to.</param>
    /// <param name="y">Vertical coordinate to move to.</param>
    public static void MoveTo(this IDrawTarget target, double x, double y) =>
        target.MoveTo(new Vector2((float)x, (float)y));

    /// <summary>
    /// Draw a line from the current point to a given point.
    /// </summary>
    /// <param name="x">Horizontal coordinate to draw to.</param>
    /// <param name="y">Vertical coordinate to draw to.</param>
    public static void LineTo(this IDrawTarget target, double x, double y) =>
        target.LineTo(new Vector2((float)x, (float)y));

    /// <summary>
    /// Draw a parabolic bezier curve
    /// </summary>
    /// <param name="controlX">Horizontal component of the control point</param>
    /// <param name="controlY">Vertical component of the control point</param>
    /// <param name="finalX">Horizontal component of the final point</param>
    /// <param name="finalY">Vertical component of the final point</param>
    public static void ConicCurveTo(this IDrawTarget target, 
        double controlX, double controlY, double finalX, double finalY) =>
        target.ConicCurveTo(new Vector2((float)controlX, (float)controlY), 
            new Vector2((float)finalX, (float)finalY));

    /// <summary>
    /// Draw a cubic bezier curve.
    /// </summary>
    /// <param name="control1X">Horizontal component of the first control point</param>
    /// <param name="control1Y">Vertical component of the first control point</param>
    /// <param name="control2X">Horizontal component of the second control point</param>
    /// <param name="control2Y">Vertical component of the second control point</param>
    /// <param name="finalX">Horizontal component of the final point</param>
    /// <param name="finalY">Vertical component of the final point</param>
    public static void CurveTo(this IDrawTarget target, 
        double control1X, double control1Y, double control2X, double control2Y,
        double finalX, double finalY) =>
        target.CurveTo(
            new Vector2((float)control1X, (float)control1Y), 
            new Vector2((float)control2X, (float)control2Y), 
            new Vector2((float)finalX, (float)finalY));
}