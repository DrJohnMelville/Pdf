using System.IO;
using System.Threading.Tasks;
using Melville.JpegLibrary.PipeAmdStreamAdapters;
using Melville.JpegLibrary.Readers;
using Melville.Pdf.LowLevel;
using Melville.Pdf.LowLevel.Parsing.ParserContext;

namespace Performance.Playground.Encryption;

public class LoadV6File
{
    public async Task TestParserAsync()
    {
        await new StreamReader(
                await new JpegStreamFactory().FromStreamAsync(
                    File.Open(@"C:\Users\jmelv\Documents\Scratch\questionable Jpeg.jpg", FileMode.Open)))
            .ReadToEndAsync();
    }
}