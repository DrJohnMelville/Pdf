using System;
using System.Numerics;
using System.Threading.Tasks;
using Melville.Pdf.Model.Renderers.FontRenderings.Type3;

namespace Melville.Pdf.Model.Renderers.FontRenderings;

/// <summary>
/// This interface manages a single font writing operation.
/// </summary>
public interface IFontWriteOperation: IDisposable
{
    /// <summary>
    /// Add a glyph outline to the current string.
    /// </summary>
    /// <param name="character"></param>
    /// <param name="glyph">Index of the glyph to render.</param>
    /// <param name="textMatrix">The current text matrix</param>
    /// <returns>The width of the rendered glyph</returns>
    ValueTask AddGlyphToCurrentStringAsync(uint character, uint glyph, Matrix3x2 textMatrix);

    /// <summary>
    /// Get the native width of the last rendered glyph in text units.
    ///
    /// Glyph must be the last glyph rendered.  Some fonts compute the width as a separate
    /// operation, whereas others compute the width as part of the AddGlyphToCurrentString
    /// operation and just return the cached width from the draw operation 
    /// </summary>
    /// <param name="glyph"></param>
    /// <returns></returns>
    ValueTask<double> NativeWidthOfLastGlyph(uint glyph);

    /// <summary>
    /// Render the glyph outlines previously added to this object.
    /// </summary>
    /// <param name="stroke">If true, the outline will be stroked with the stroke brush.</param>
    /// <param name="fill">If true, the outline will be filled with the fill brush.</param>
    /// <param name="clip">If true, the string will be added to the current clipping region.</param>
    /// <param name="finalTextMatrix">Value of the text matrix at the end of the write operation</param>
    void RenderCurrentString(bool stroke, bool fill, bool clip, in Matrix3x2 finalTextMatrix);

    /// <summary>
    /// Create an IFontWriteOperation with a different target;
    /// </summary>
    /// <param name="target">The target that the new font write operation should write to.</param>
    /// <returns>A writeroperation for the same font with a new target.</returns>
    IFontWriteOperation CreatePeerWriteOperation(IFontTarget target);
}