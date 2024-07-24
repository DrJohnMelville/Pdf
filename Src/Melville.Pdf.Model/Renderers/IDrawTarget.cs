using System;
using System.Numerics;
using Melville.Fonts;

namespace Melville.Pdf.Model.Renderers;

/// <summary>
/// Target of a PDF drawing operation
/// </summary>
public interface IDrawTarget: IDisposable, IGlyphTarget
{
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