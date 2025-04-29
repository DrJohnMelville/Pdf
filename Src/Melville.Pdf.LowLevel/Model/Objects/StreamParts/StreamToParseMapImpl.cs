using System.IO;
using System.Threading.Tasks;
using Melville.Parsing.AwaitConfiguration;
using Melville.Parsing.ParserMapping;

namespace Melville.Pdf.LowLevel.Model.Objects.StreamParts;

public static class StreamToParseMapImpl
{
    public static async ValueTask SetDataAsync(this ParseMap? map, PdfStream s)
    {
        if (map == null) return;
        await map.SetDataAsync(await s.StreamContentAsync().CA()).CA();
    }
}