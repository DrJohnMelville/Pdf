using System.Buffers;
using System.IO.Pipelines;
using System.Runtime.CompilerServices;
using Melville.Fonts.SfntParsers.TableParserParts;
using Melville.Parsing.AwaitConfiguration;
using Melville.Parsing.MultiplexSources;
using Microsoft.CodeAnalysis;

namespace Melville.Fonts.SfntParsers.TableDeclarations.Metrics;

internal class HorizontalMetricsParser(
    PipeReader source,
    ushort numberOfHMetrics,
    ushort numGlyphs)
{
    public async Task<ParsedHorizontalMetrics> ParseAsync()
    {
        var bytes = (numberOfHMetrics * 4) + ((numGlyphs - numberOfHMetrics) * 2);
        var data = new short[bytes / 2];
        await FieldParser.ReadAsync(source, data).CA();
        return new ParsedHorizontalMetrics(data, numberOfHMetrics);
    }
}