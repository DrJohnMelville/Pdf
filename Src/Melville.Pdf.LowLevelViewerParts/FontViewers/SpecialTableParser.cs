using System.Configuration;
using System.Runtime.InteropServices.ComTypes;
using Melville.Fonts.SfntParsers;
using Melville.Fonts.SfntParsers.TableDeclarations;
using Melville.Fonts.SfntParsers.TableDeclarations.CMaps;
using Melville.Parsing.MultiplexSources;
using Melville.Pdf.LowLevelViewerParts.FontViewers.CmapViewers;
using Melville.Pdf.LowLevelViewerParts.LowLevelViewer.DocumentParts.Streams;

namespace Melville.Pdf.LowLevelViewerParts.FontViewers;

public static class SpecialTableParser
{
    public static ValueTask<object> ParseAsync(uint tag, byte[] data) =>
        tag switch
        {
            SFntTableName.CMap => ParseCmapAsync(data),
            _ => new(new ByteStringViewModel(data))
        };

    private static async ValueTask<object> ParseCmapAsync(byte[] data)
    {
        var parser = new CmapParser(MultiplexSourceFactory.Create(data));
        var cmap = await parser.ParseCmapTableAsync();
        return new CompositeTableViewModel(Enumerable.Range(0, cmap.Count)
            .Select(i => new CMapViewModel(cmap, i) as object)
            .Append(new ByteStringViewModel(data)).ToArray());
    }
}