using System.Numerics;
using Melville.Fonts.SfntParsers.TableDeclarations.Heads;
using Melville.Fonts.SfntParsers.TableDeclarations.TrueTypeGlyphs;

namespace Melville.Fonts;

/// <summary>
/// This is the generic interface that all fonts parsed by this parser return.  This is
/// used by the renderer to render characters.
/// </summary>
public interface IGenericFont
{
    // this is an interface that handles all of the font types
    /// <summary>
    /// Load the CMaps for a font.
    /// </summary>
    ValueTask<ICMapSource> GetCmapSourceAsync();

    /// <summary>
    /// This retrieves the IGlyphSource for the font.
    /// </summary>
    ValueTask<IGlyphSource> GetGlyphSourceAsync();

    /// <summary>
    /// Get an array of the names of the glyphs.  May be empty if the font does not
    /// have glyph names.
    /// </summary>
    /// <returns></returns>
    ValueTask<string[]> GlyphNamesAsync();

    /// <summary>
    /// Gets the object that holds the glyph widths.
    /// </summary>
    /// <returns></returns>
    ValueTask<IGlyphWidthSource> GlyphWidthSourceAsync();

    /// <summary>
    /// Retrieves the name of the font
    /// </summary>
    /// <returns>String name of the font, or empty string if the font is unnamed</returns>
    ValueTask<string> FontFamilyNameAsync();

    /// <summary>
    /// Gets a bitfield that represents the visual style of the font.
    /// </summary>
    /// <returns>A MacStyles enum.  This may be syntesized on non SFnt fonts.</returns>
    ValueTask<MacStyles> GetFontStyleAsync();

    /// <summary>
    /// Pdf uses different CID mapping styles that differ based on the kind of font in use
    /// </summary>
    public CidToGlyphMappingStyle TypeGlyphMapping { get; }

}

/// <summary>
/// Represents different CID to glyph mapping rules that should be for differnt font types
/// </summary>
public enum CidToGlyphMappingStyle
{
     CFF = 0, // CFF font maping is stored in the single 
     TrueType = 0,
}