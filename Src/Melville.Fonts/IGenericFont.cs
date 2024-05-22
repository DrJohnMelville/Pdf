using Melville.Fonts.SfntParsers.TableDeclarations.CMaps;

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
    ValueTask<ICMapSource> ParseCMapsAsync();
}

/// <summary>
/// This interface represents the CMaps or character to glyph mappings available within
/// a font.
/// </summary>
public interface ICMapSource
{
    /// <summary>
    /// Number of CMaps supported
    /// </summary>
    int Count { get; }

    /// <summary>
    /// Load a CMap implementation by index.
    /// </summary>
    /// <param name="index">The index of the desired Cmap</param>
    /// <returns>An object that implements the given CMAP</returns>
    ValueTask<ICmapImplementation> GetByIndexAsync(int index);

    /// <summary>
    /// Load a CMap implementation by platform and encoding
    /// </summary>
    /// <param name="platform">The Platform encoding that was desired</param>
    /// <param name="encoding">The desired encoding for the platform</param>
    /// <returns>An object that implements the given CMAP, or null if there is none</returns>
    ValueTask<ICmapImplementation?> GetByPlatformEncodingAsync(int platform, int encoding);

    /// <summary>
    /// Get the platform and encoding for a given subtable.
    /// </summary>
    /// <param name="index">The index of the subtable to get information from</param>
    /// <returns>The platform and encoding values for the indicated CMap</returns>
    (int platform, int encoding) GetPlatformEncoding(int index);

}
