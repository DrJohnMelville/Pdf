using System.Buffers;
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

namespace Melville.Pdf.LowLevelViewerParts.FontViewers;

public static class SpecialTableParser
{
    public static ValueTask<object> ParseAsync(uint tag, byte[] data) =>
        tag switch
        {
            SFntTableName.CMap => ParseCmapAsync(data),
            SFntTableName.Head => ParseHeadAsync(data),
            SFntTableName.HorizontalHeadder => ParseHorizontalHeaderAsync(data),
            SFntTableName.MaximumProfile => ParsedMaximumsProfileAsync(data),
            _ => new(new ByteStringViewModel(data))
        };

    private static async ValueTask<object> ParseCmapAsync(byte[] data)
    {
        var cmap = await TableLoader.LoadCmapAsync(MultiplexSourceFactory.Create(data));
        return new CompositeTableViewModel(Enumerable.Range(0, cmap.Count)
            .Select(i => new CMapViewModel(cmap, i) as object)
            .Append(new ByteStringViewModel(data)).ToArray());
    }

    private static async ValueTask<object> ParseHeadAsync(byte[] data)
    {
        var head = new HeadViewModel(await TableLoader.LoadHeadAsync(MultiplexSourceFactory.Create(data)));
        return TwoTabModel(head, data);
    }

    private static CompositeTableViewModel TwoTabModel(object head, byte[] data) =>
        new([head, new ByteStringViewModel(data)]);

    private static async ValueTask<object> ParseHorizontalHeaderAsync(byte[] data) =>
        TwoTabModel(
            new HorizontalHeaderViewModel(await TableLoader.LoadHorizontalHeaderAsync(MultiplexSourceFactory.Create(data))),
            data);

    private static async ValueTask<object> ParsedMaximumsProfileAsync(byte[] data) =>
        TwoTabModel(
            new MaximumsViewModel(await TableLoader.LoadMaximumProfileAsync(MultiplexSourceFactory.Create(data))), data);
}