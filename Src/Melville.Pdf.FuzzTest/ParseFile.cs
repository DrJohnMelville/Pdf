using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Parsing.FileParsers;
using Melville.Pdf.LowLevel.Visitors;
using Melville.Pdf.Model.Documents;
using Melville.Pdf.Model.Renderers;
using Melville.Pdf.Model.Renderers.FontRenderings.DefaultFonts;
using Melville.Pdf.SkiaSharp;

namespace Melville.Pdf.FuzzTest;

public static class ParseFile
{
    public static async ValueTask Do(string fileName)
    {
        await using var stream = File.OpenRead(fileName);
        await Do(stream);
    }

    private static async ValueTask Do(FileStream source)
    {
        try
        {
            using var doc = await DocumentRendererFactory.CreateRendererAsync(
                await PdfDocument.ReadAsync(source), WindowsDefaultFonts.Instance);
            for (int i = 0; i < doc.TotalPages; i++)
            {
                Console.Write(".");
                await RenderPage(doc, i);
            }
        }
        catch (Exception e)
        {
            OutputException(-1, e);
        }
    }

    private static async Task RenderPage(DocumentRenderer doc, int page)
    {
        try
        {
            await RenderWithSkia.ToSurface(doc, page);
        }
        catch (Exception e)
        {
            OutputException(page, e);
        }
    }

    private static void OutputException(int page, Exception e)
    {
        Console.WriteLine();
        Console.WriteLine($"Page {page}");
        Console.WriteLine(e.Message);
    }
}