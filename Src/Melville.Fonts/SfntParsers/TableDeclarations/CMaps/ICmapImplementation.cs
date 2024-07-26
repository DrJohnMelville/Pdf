namespace Melville.Fonts.SfntParsers.TableDeclarations.CMaps;

/// <summary>
/// This is the functional mapping for a Cmap table.
/// </summary>
public interface ICmapImplementation
{
    /// <summary>
    /// Return all the mappings expressed by this cmap
    /// </summary>
    /// <returns>An enumerable of all the mappings each composed of a byte length
    /// character and the resulting glyph</returns>
    IEnumerable<(int Bytes, uint Character, uint Glyph)> AllMappings();
    
    /// <summary>
    /// Try to map a given number of bytes and a character to a glyph
    /// </summary>
    /// <param name="bytes">the number of bytes in character that are filled in</param>
    /// <param name="character">the character value.</param>
    /// <param name="glyph">The corresponding glyph value, or zero if there is no glypg</param>
    /// <returns>True if bytes/character does not need more bytes for a mappping, false otherwise.  </returns>
    bool TryMap(int bytes, uint character, out uint glyph);

    /// <summary>
    /// Map a character to a glyph
    /// </summary>
    /// <param name="character">The character to be mapped</param>
    /// <returns>The corresponding glyph</returns>
    uint Map(uint character)
    {
        TryMap(4, character, out var ret);
        return ret;
    }
}