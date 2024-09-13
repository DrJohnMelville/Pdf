using System;
using System.IO;
using System.Threading.Tasks;
using Melville.Pdf.ImageExtractor;
using Melville.Pdf.Model;
using Melville.Pdf.SkiaSharp;
using Melville.Pdf.TextExtractor;

namespace Performance.Playground;

public class Bugs
{
    public async Task ReadAllImages()
    {
        var doc = await new PdfReader().ReadFromFileAsync(@"C:\Users\jom252\OneDrive - Medical University of South Carolina\Documents\Scratch\Pdf\BUGS\num 41.pdf");
        await RenderWithSkia.ToPngStreamAsync(doc, 1, new MemoryStream());
    }
}