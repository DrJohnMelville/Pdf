using System.IO.Pipelines;
using Melville.Fonts.SfntParsers.TableDeclarations.CMaps;
using Melville.Fonts.SfntParsers.TableDeclarations.Heads;
using Melville.Parsing.AwaitConfiguration;
using Melville.Parsing.MultiplexSources;

namespace Melville.Fonts.SfntParsers.TableParserParts;

/// <summary>
/// This class needs to be public so the font viewer can parse tables from
/// fragments of a font file.
/// </summary>
public static class TableLoader
{
    public static async ValueTask<ICMapSource> LoadCmap(IMultiplexSource source) =>
        new ParsedCmap(source,
            (await FieldParser.ReadFromAsync<CmapTable>(source.ReadPipeFrom(0)).CA()).Tables);

    public static async ValueTask<ParsedHead> LoadHead(IMultiplexSource source) =>
        await FieldParser.ReadFromAsync<ParsedHead>(source.ReadPipeFrom(0)).CA();
}