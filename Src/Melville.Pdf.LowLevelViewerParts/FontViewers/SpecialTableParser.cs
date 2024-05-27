using System.Buffers;
using System.Collections;
using System.Configuration;
using System.IO;
using System.IO.Pipelines;
using System.Runtime.InteropServices.ComTypes;
using Melville.Fonts.SfntParsers;
using Melville.Fonts.SfntParsers.TableDeclarations;
using Melville.Fonts.SfntParsers.TableDeclarations.CMaps;
using Melville.Fonts.SfntParsers.TableDeclarations.Heads;
using Melville.Fonts.SfntParsers.TableDeclarations.Maximums;
using Melville.Fonts.SfntParsers.TableDeclarations.Metrics;
using Melville.Fonts.SfntParsers.TableParserParts;
using Melville.Parsing.MultiplexSources;
using Melville.Pdf.LowLevelViewerParts.FontViewers.CmapViewers;
using Melville.Pdf.LowLevelViewerParts.FontViewers.HeadViewers;
using Melville.Pdf.LowLevelViewerParts.FontViewers.MetricViewers;
using Melville.Pdf.LowLevelViewerParts.LowLevelViewer.DocumentParts.Streams;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Melville.Pdf.LowLevelViewerParts.FontViewers;

public static class SpecialTableParser
{
    public static async ValueTask<object> ParseAsync(TableRecord record, SFnt font)
    {

        var byteModel = new ByteStringViewModel(await font.GetTableBytesAsync(record));
        var special = await ParseAsync(record.Tag, font);

        return special switch
        {
            IEnumerable enumerable => new CompositeTableViewModel([..enumerable, byteModel]),
            { } x => new CompositeTableViewModel(new object[] {x, byteModel}),
            _ => byteModel
        };
    }

    private static ValueTask<object?> ParseAsync(uint tag, SFnt font) =>
        tag switch
        {
            SFntTableName.CMap => ParseCmapAsync(font),
            SFntTableName.Head => ParseHeadAsync(font),
            SFntTableName.HorizontalHeadder => ParseHorizontalHeaderAsync(font),
            SFntTableName.MaximumProfile => ParsedMaximumsProfileAsync(font),
            SFntTableName.HorizontalMetrics => ParseHorizontalMetrics(font),
            _ => new ValueTask<object?>((object?)null)
        };

    private static async ValueTask<object?> ParseCmapAsync(SFnt font)
    {
        var cmap = await font.ParseCMapsAsync();
        return Enumerable.Range(0, cmap.Count)
            .Select(i => new CMapViewModel(cmap, i) as object);
    }
    private static async ValueTask<object?> ParseHeadAsync(SFnt font) => 
        new HeadViewModel(await font.HeadTableAsync());

    private static async ValueTask<object?> ParseHorizontalHeaderAsync(SFnt font) =>
            new HorizontalHeaderViewModel(await font.HorizontalHeaderTableAsync());

    private static async ValueTask<object?> ParsedMaximumsProfileAsync(SFnt font) =>
            new MaximumsViewModel(await font.MaximumProfileTableAsync());

    private static async ValueTask<object?> ParseHorizontalMetrics(SFnt font) =>
        new HorizontalMetricsViewModel(await font.HorizontalMetrics());
}