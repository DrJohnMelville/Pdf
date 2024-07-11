using System.Numerics;
using Melville.INPC;

namespace Melville.Fonts;

/// <summary>
/// This interface receives the instructions that form a glyph outline.
/// </summary>
public interface IGlyphTarget
{
    /// <summary>
    /// Begin a new contour the given location.
    /// </summary>
    /// <param name="point">A scaled point to move the current position</param>
    void MoveTo(Vector2 point);
    
    /// <summary>
    /// Draw a straight line from the previous location to this location and set
    /// the current location to this location.
    /// </summary>
    /// <param name="point">The endpoint of the line segment to draw</param>
    void LineTo(Vector2 point);

    /// <summary>
    /// Draws a cubic Bezier curve from the current location to the endpoint with the
    /// given control points.
    /// </summary>
    /// <param name="control">Control point</param>
    /// <param name="endPoint">End point</param>
    void CurveTo(Vector2 control, Vector2 endPoint);

    /// <summary>
    /// Draws a Bezier curve from the current location to the endpoint with the
    /// given control points.
    /// </summary>
    /// <param name="control1">First control point</param>
    /// <param name="control2">Second control point</param>
    /// <param name="endPoint">End point</param>
    void CurveTo(Vector2 control1, Vector2 control2, Vector2 endPoint);

    /// <summary>
    /// Indicates that no more instructions that will be for this character
    /// </summary>
    void EndGlyph();

}

/// <summary>
/// This is a GlyphTarget that does nothing.  It is used for testing and as a default
/// </summary>
[StaticSingleton]
public partial class NullTarget : IGlyphTarget
{
    /// <inheritdoc />
    public void MoveTo(Vector2 point)
    {
    }

    /// <inheritdoc />
    public void LineTo(Vector2 point)
    {
    }

    /// <inheritdoc />
    public void CurveTo(Vector2 control, Vector2 endPoint)
    {
    }

    /// <inheritdoc />
    public void CurveTo(Vector2 control1, Vector2 control2, Vector2 endPoint)
    {
    }

    /// <inheritdoc />
    public void EndGlyph()
    {
    }
}