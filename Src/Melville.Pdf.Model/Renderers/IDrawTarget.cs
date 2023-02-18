using System;
using System.Numerics;

namespace Melville.Pdf.Model.Renderers;

/// <summary>
/// Target of a PDF drawing operation
/// </summary>
public interface IDrawTarget: IDisposable
{
    /// <summary>
    /// Set tje drawing transform.
    /// </summary>
    void SetDrawingTransform(in Matrix3x2 transform);

    /// <summary>
    /// Move the current point to a position.
    /// </summary>
    /// <param name="x">Horizontal coordinate to move to.</param>
    /// <param name="y">Vertical coordinate to move to.</param>
    void MoveTo(double x, double y);
    
    /// <summary>
    /// Draw a line from the current point to a given point.
    /// </summary>
    /// <param name="x">Horizontal coordinate to draw to.</param>
    /// <param name="y">Vertical coordinate to draw to.</param>
    void LineTo(double x, double y);

    /// <summary>
    /// Draw a parabolic bezier curve
    /// </summary>
    /// <param name="controlX">Horizontal component of the control point</param>
    /// <param name="controlY">Vertical component of the control point</param>
    /// <param name="finalX">Horizontal component of the final point</param>
    /// <param name="finalY">Vertical component of the final point</param>
    void ConicCurveTo(double controlX, double controlY, double finalX, double finalY);

    /// <summary>
    /// Draw a cubic bezier curve.
    /// </summary>
    /// <param name="control1X">Horizontal component of the first control point</param>
    /// <param name="control1Y">Vertical component of the first control point</param>
    /// <param name="control2X">Horizontal component of the second control point</param>
    /// <param name="control2Y">Vertical component of the second control point</param>
    /// <param name="finalX">Horizontal component of the final point</param>
    /// <param name="finalY">Vertical component of the final point</param>
    void CurveTo(double control1X, double control1Y, double control2X, double control2Y,
        double finalX, double finalY);

    /// <summary>
    /// Draw a line from the current point to the beginning of this polycurve, and close it.
    /// </summary>
    void ClosePath();

    /// <summary>
    /// Paint the current path.
    /// </summary>
    /// <param name="stroke">Draw the outline with the current stroking brush.</param>
    /// <param name="fill">Fill the shape with the current nonstroking brush.</param>
    /// <param name="evenOddFillRule">True to use the even odd rule, false to use the winding rule.</param>
    void PaintPath(bool stroke, bool fill, bool evenOddFillRule);

    /// <summary>
    ///  Add the current path to the current clipping region.
    /// </summary>
    /// <param name="evenOddRule">True to use the even odd rule, false to use the winding rule.</param>
    void ClipToPath(bool evenOddRule);
}