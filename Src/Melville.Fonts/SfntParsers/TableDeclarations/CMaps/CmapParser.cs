using Melville.Fonts.SfntParsers.TableParserParts;
using Melville.Parsing.AwaitConfiguration;
using Melville.Parsing.MultiplexSources;

namespace Melville.Fonts.SfntParsers.TableDeclarations.CMaps;

/// <summary>
/// Read a Cmap Table
/// </summary>
/// <param name="source">The IMultiplexSource that holds the cmap data</param>
public readonly struct CmapParser(IMultiplexSource source)
{
    /// <summary>
    /// Read a ParsedCmap from the source.
    /// </summary>
    /// <returns>A ParsedCmap representing this table.</returns>
    public async ValueTask<ParsedCmap> ParseCmapTableAsync()
    {
        var table = await FieldParser.ReadFromAsync<CmapTable>(source.ReadPipeFrom(0)).CA();
        return new ParsedCmap(source, table.Tables);
    }
}