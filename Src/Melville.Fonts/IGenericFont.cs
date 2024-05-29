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
}

/// <summary>
/// This interface is used to retrieve individual glyphs from the font file  
/// </summary>
public interface IGlyphSource
{
    /// <summary>
    /// The number of glyphs in the font.
    /// </summary>
    int GlyphCount { get; }
}
