using System;
using Melville.Pdf.LowLevel.Model.ContentStreams;
using Melville.Pdf.Model.Renderers.FontRenderings.Type3;

namespace Melville.Pdf.Model.Renderers.FontRenderings;

/// <summary>
/// This object represents a font that can map strings to glyphs and render glyphs to a target object
/// </summary>
public interface IRealizedFont
{
    /// <summary>
    /// gather the next glyph from an input span.  This could be one or more bytes depending on the font.
    /// </summary>
    /// <param name="input">The input to read from.</param>
    /// <returns>The character index, glyph index, and number of bytes consumed.</returns>
    (uint character, uint glyph, int bytesConsumed) GetNextGlyph(in ReadOnlySpan<byte> input);
    /// <summary>
    /// The width, in text units, of a given character.  This is used to adjust the text matrix in a write operation.
    /// </summary>
    /// <param name="character">The character to be rendered.</param>
    /// <param name="defaultWidth">The measured width of the character.</param>
    /// <returns>The effective width of the character, usually taken from the PDF font definition.</returns>
    double CharacterWidth(uint character, double defaultWidth);
    /// <summary>
    /// Begin writing a string to a target.
    /// </summary>
    /// <param name="target">A drawing target for the font write operation.</param>
    /// <returns>An IFontWriteOperation that will render this font to the given target..</returns>
    IFontWriteOperation BeginFontWrite(IFontTarget target);

    /// <summary>
    /// The number of glyphs in the font.
    /// </summary>
    int GlyphCount { get; }
    /// <summary>
    /// Family name of the font -- if there is one.
    /// </summary>
    string FamilyName { get; }
    /// <summary>
    /// A descriptive string of information the font wants to expose to the low level UI
    /// </summary>
    string Description { get; }

    /// <summary>
    /// Returns true if the font rendering is invariant of the context in which is is rendered.
    /// Type 3 fonts can pick up different resources on different pages, so they cannot be cached.
    /// </summary>
    bool IsCachableFont { get; }
}

internal static class FontWriteOperationsImpl
{
    public static void RenderCurrentString(this IFontWriteOperation op, TextRendering rendering)
    {
        switch (rendering)
        {
            case TextRendering.Fill:
                op.RenderCurrentString(false, true, false);
                break;
            case TextRendering.Stroke:
                op.RenderCurrentString(true, false, false);
                break;
            case TextRendering.FillAndStroke:
                op.RenderCurrentString(true, true, false);
                break;
            case TextRendering.Invisible:
                op.RenderCurrentString(false, false, false);
                break;
            case TextRendering.FillAndClip:
                op.RenderCurrentString(false, true, true);
                break;
            case TextRendering.StrokeAndClip:
                op.RenderCurrentString(true, false, true);
                break;
            case TextRendering.FillStrokeAndClip:
                op.RenderCurrentString(true, true, true);
                break;
            case TextRendering.Clip:
                op.RenderCurrentString(false, false, true);
                break;
        }
    }
}