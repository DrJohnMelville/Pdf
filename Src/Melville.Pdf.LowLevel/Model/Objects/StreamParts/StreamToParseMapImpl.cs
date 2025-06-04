using System.IO;
using System.Threading.Tasks;
using Melville.Parsing.AwaitConfiguration;
using Melville.Parsing.ParserMapping;

namespace Melville.Pdf.LowLevel.Model.Objects.StreamParts;

/// <summary>
/// This extension method is used to set the data of a ParseMap from a PdfStream.
/// </summary>
public static class StreamToParseMapImpl
{
    /// <summary>
    /// Sets the data of a ParseMap from a PdfStream. This is only used in debug mode
    /// </summary>
    /// <param name="map">The parsemap to det the data for </param>
    /// <param name="s">The stream that is the source of the data the parsemap will parse</param>
#if DEBUG
    public static async ValueTask SetDataAsync(this ParseMap? map, PdfStream s)
    {
        if (map == null) return;
        await map.SetDataAsync(await s.StreamContentAsync().CA()).CA();
    }
#else
    public static ValueTask SetDataAsync(this ParseMap? map, PdfStream s) =>
        ValueTask.CompletedTask;
#endif
}
