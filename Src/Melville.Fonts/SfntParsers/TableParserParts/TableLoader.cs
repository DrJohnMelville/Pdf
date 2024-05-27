using System.IO.Pipelines;
using Melville.Fonts.SfntParsers.TableDeclarations.CMaps;
using Melville.Fonts.SfntParsers.TableDeclarations.Heads;
using Melville.Fonts.SfntParsers.TableDeclarations.Maximums;
using Melville.Fonts.SfntParsers.TableDeclarations.Metrics;
using Melville.Parsing.AwaitConfiguration;
using Melville.Parsing.MultiplexSources;

namespace Melville.Fonts.SfntParsers.TableParserParts;

/// <summary>
/// This class needs to be public so the font viewer can parse tables from
/// fragments of a font file.
/// </summary>
public static class TableLoader
{
    /// <summary>
    /// Load the CMAP table from a byte strea,
    /// </summary>
    /// <param name="source">The source information for the table</param>
    public static async ValueTask<ICMapSource> LoadCmap(IMultiplexSource source) =>
        new ParsedCmap(source,
            (await FieldParser.ReadFromAsync<CmapTable>(source.ReadPipeFrom(0)).CA()).Tables);

    /// <summary>
    /// Load the head table from a byte stream
    /// </summary>
    /// <param name="source">The source information for the table</param>
    public static async ValueTask<ParsedHead> LoadHead(IMultiplexSource source) =>
        await FieldParser.ReadFromAsync<ParsedHead>(source.ReadPipeFrom(0)).CA();

    /// <summary>
    /// Load the hhea table from a byte stream
    /// </summary>
    /// <param name="source">The source information for the table</param>
    public static ValueTask<ParsedHorizontalHeader> LoadHorizontalHeader(
        IMultiplexSource create) =>
        FieldParser.ReadFromAsync<ParsedHorizontalHeader>(create.ReadPipeFrom(0));

    public static ValueTask<ParsedMaximums> LoadMaximumProfile(IMultiplexSource create) => 
        new MaxpParser(create.ReadPipeFrom(0)).ParseAsync();
}