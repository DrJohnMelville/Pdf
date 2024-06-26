using System.Numerics;

namespace Melville.Fonts.SfntParsers.TableDeclarations.TrueTypeGlyphs;

/// <summary>
/// Receives the points that make up a TrueType Glyph outline.
/// </summary>
public interface ITrueTypePointTarget
{
    /// <summary>
    /// Add a real point to the glyph
    /// </summary>
    /// <param name="point">Location of the point</param>
    /// <param name="onCurve">True if this is a curve point, false if it is a control point</param>
    /// <param name="isContourStart">True if this is the first point in a contour, false otherwise.</param>
    /// <param name="isContourEnd">True if this is the last point in a contour, false otherwise</param>
    void AddPoint(Vector2 point, bool onCurve, bool isContourStart, bool isContourEnd);
    /// <summary>
    /// Add a phantom point, which does not render but can affect the shape of composite glyphs.
    /// </summary>
    /// <param name="point"></param>
    void AddPhantomPoint(Vector2 point);
}