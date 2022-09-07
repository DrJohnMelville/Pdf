using System.IO;
using System.Threading.Tasks;
using Melville.Pdf.Model;
using Melville.Pdf.SkiaSharp;

namespace Melville.Wpf.IntegrationTesting;

public class RenderingExample
{

    public async Task DoIt()
    {
        var document = await new PdfReader().ReadFromFile("File.Pdf");
        var output = new MemoryStream();
        await RenderWithSkia.ToPngStreamAsync(document, 1, output);
    }
}