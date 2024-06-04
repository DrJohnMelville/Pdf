using System.Buffers;
using System.Collections;
using System.Configuration;
using System.IO;
using System.IO.Pipelines;
using System.Printing;
using System.Runtime.InteropServices.ComTypes;
using Melville.Fonts.SfntParsers;
using Melville.Fonts.SfntParsers.TableDeclarations;
using Melville.Fonts.SfntParsers.TableDeclarations.CMaps;
using Melville.Fonts.SfntParsers.TableDeclarations.Heads;
using Melville.Fonts.SfntParsers.TableDeclarations.Maximums;
using Melville.Fonts.SfntParsers.TableDeclarations.Metrics;
using Melville.Fonts.SfntParsers.TableParserParts;
using Melville.Parsing.MultiplexSources;
using Melville.Pdf.LowLevelViewerParts.FontViewers.GlyphViewer;
using Melville.Pdf.LowLevelViewerParts.FontViewers.HeadViewers;
using Melville.Pdf.LowLevelViewerParts.FontViewers.MetricViewers;
using Melville.Pdf.LowLevelViewerParts.LowLevelViewer.DocumentParts.Streams;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Melville.Pdf.LowLevelViewerParts.FontViewers;

public static class SpecialTableParser
{
    public static async ValueTask<object> ParseAsync(TableRecord record, SFnt font)
    {
        if (record.Tag == SFntTableName.GlyphData)
        {
            return new GlyphsViewModel(await font.GetGlyphSourceAsync());
        }

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
            SFntTableName.HorizontalMetrics => ParseHorizontalMetricsAsync(font),
            SFntTableName.GlyphLocations => ParseLocationsAsync(font),
            _ => new ValueTask<object?>((object?)null)
        };

    private static async ValueTask<object?> ParseCmapAsync(SFnt font)
    {
        var cmap = await font.GetCmapSourceAsync();
        return Enumerable.Range(0, cmap.Count)
            .Select(i => new MultiStringViewModel(()=> PrintCmap.PrintCMapAsync(cmap, i), PrintCmap.CmapName(cmap, i)) as object);
    }

    private static async ValueTask<object?> ParseHeadAsync(SFnt font) => 
        new HeadViewModel(await font.HeadTableAsync());

    private static async ValueTask<object?> ParseHorizontalHeaderAsync(SFnt font) =>
            new HorizontalHeaderViewModel(await font.HorizontalHeaderTableAsync());

    private static async ValueTask<object?> ParsedMaximumsProfileAsync(SFnt font) =>
            new MaximumsViewModel(await font.MaximumProfileTableAsync());

    private static ValueTask<object?> ParseHorizontalMetricsAsync(SFnt font) =>
        new(new MultiStringViewModel(async ()=> await MetricsAsync(await font.HorizontalMetricsAsync()), "Horizontal Metrics"));

    public static ValueTask<IReadOnlyList<string>> MetricsAsync(ParsedHorizontalMetrics metrics)
    {
        var ret = new List<string>(metrics.HMetrics.Length + metrics.LeftSideBearings.Length + 1);
        int glyph = 0;
        ushort lastWidth = 0;
        foreach (var metric in metrics.HMetrics)
        {
            lastWidth = metric.AdvanceWidth;
            ret.Add($"0x{glyph++:X} => ({lastWidth}, {metric.LeftSideBearing})");
        }
        ret.Add("Implicit Metrics");
        foreach (var bearing in metrics.LeftSideBearings)
        {
            ret.Add($"{glyph++:X} => ({lastWidth}, {bearing})");
            
        }
        return new (ret);
    }

    private static ValueTask<object?> ParseLocationsAsync(SFnt font) => new(new MultiStringViewModel(() => PrintGlyphLocationsAsync(font), "Glyph Locations"));

    private static async ValueTask<IReadOnlyList<string>> PrintGlyphLocationsAsync(SFnt font)
    {
        var table = await font.GlyphLocationsAsync();
        if (table is null) return [];
        return Enumerable.Range(0, table.TotalGlyphs)
            .Select(i =>
            {
                var loc = table.GetLocation((uint)i);
                return $"{i:X} = ({loc.Offset}, {loc.Length})";
            }).ToList();
    }
}