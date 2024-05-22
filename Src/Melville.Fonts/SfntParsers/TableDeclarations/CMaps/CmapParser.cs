using Melville.Fonts.SfntParsers.TableParserParts;
using Melville.Parsing.AwaitConfiguration;
using Melville.Parsing.MultiplexSources;

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

/// <summary>
/// Read a Cmap Table
/// </summary>
/// <param name="source">The IMultiplexSource that holds the cmap data</param>
public readonly struct CmapParser(IMultiplexSource source)
{
    /// <summary>
    /// Read a ParsedCmap from the source.
    /// </summary>
    /// <returns>A, ICMapSource representing this table.</returns>
    public async ValueTask<ICMapSource> ParseCmapTableAsync()
    {
        var table = await FieldParser.ReadFromAsync<CmapTable>(source.ReadPipeFrom(0)).CA();
        return new ParsedCmap(source, table.Tables);
    }
}