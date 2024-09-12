using System;
using System.IO;
using System.Threading.Tasks;
using Melville.Pdf.ImageExtractor;
using Melville.Pdf.Model;
using Melville.Pdf.TextExtractor;

namespace Performance.Playground;

public class Bugs
{
    public async Task ReadAllImages()
    {
        var bytes =
            File.ReadAllBytes(
                @"C:\Users\jom252\OneDrive - Medical University of South Carolina\Documents\Scratch\Pdf\BUGS\1483757-2022-10.pdf");
        var doc = await new PdfReader().ReadFromAsync(bytes);
        var text = await doc.PageTextAsync(1); // if you comment this out everything works
        var ret = doc.ImagesFromAsync();
        await foreach (var image in ret)
        {
            GC.KeepAlive(image);
        }
    }
}