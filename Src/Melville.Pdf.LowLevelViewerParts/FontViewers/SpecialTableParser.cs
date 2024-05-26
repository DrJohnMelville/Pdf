using System.Buffers;
using System.Configuration;
using System.IO;
using System.IO.Pipelines;
using System.Runtime.InteropServices.ComTypes;
using Melville.Fonts.SfntParsers;
using Melville.Fonts.SfntParsers.TableDeclarations;
using Melville.Fonts.SfntParsers.TableDeclarations.CMaps;
using Melville.Fonts.SfntParsers.TableDeclarations.Heads;
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
            SFntTableName.HorizontalHeadder => ParseHorizontalHeader(data),
            _ => new(new ByteStringViewModel(data))
        };

    private static async ValueTask<object> ParseCmapAsync(byte[] data)
    {
        var cmap = await TableLoader.LoadCmap(MultiplexSourceFactory.Create(data));
        return new CompositeTableViewModel(Enumerable.Range(0, cmap.Count)
            .Select(i => new CMapViewModel(cmap, i) as object)
            .Append(new ByteStringViewModel(data)).ToArray());
    }

    private static async ValueTask<object> ParseHeadAsync(byte[] data)
    {
        var head = await TableLoader.LoadHead(MultiplexSourceFactory.Create(data));
        return new CompositeTableViewModel(
            [new HeadViewModel(head), new ByteStringViewModel(data)]);
    }

    private static async ValueTask<object> ParseHorizontalHeader(byte[] data)
    {
        var header = await TableLoader.LoadHorizontalHeader(MultiplexSourceFactory.Create(data));
        return new CompositeTableViewModel(
            [new HorizontalHeaderViewModel(header), new ByteStringViewModel(data)]);
    }

}