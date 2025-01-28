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
    public async Task ReadAllImagesAsync()
    {
        var doc = await new PdfReader().ReadFromFileAsync(@"C:\Users\jom252\OneDrive - Medical University of South Carolina\Documents\Scratch\PrintTarget\zpe00811000407.pdf");

        var text = await doc.PageTextAsync(1);
        Console.WriteLine(text);
    }
}