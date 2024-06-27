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
    /// <param name="startPoint">Point to move to.</param>
    void MoveTo(Vector2 startPoint);
    
    /// <summary>
    /// Draw a line from the current point to a given point.
    /// </summary>
    /// <param name="endPoint">Point to move to.</param>
    void LineTo(Vector2 endPoint);

    /// <summary>
    /// Draw a parabolic bezier curve
    /// </summary>
    /// <param name="control">The control point</param>
    /// <param name="final">The final point</param>
    void ConicCurveTo(Vector2 control, Vector2 final);

    /// <summary>
    /// Draw a cubic bezier curve.
    /// </summary>
    /// <param name="control1">The first control point</param>
    /// <param name="control2">The second control point</param>
    /// <param name="final">The final point</param>
    void CurveTo(Vector2 control1, Vector2 control2, Vector2 final);

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