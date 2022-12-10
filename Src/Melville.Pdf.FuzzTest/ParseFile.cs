using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Xml.Linq;
using Melville.Pdf.LowLevel.Parsing.FileParsers;
using Melville.Pdf.LowLevel.Visitors;
using Melville.Pdf.Model.Documents;
using Melville.Pdf.Model.Renderers;
using Melville.Pdf.Model.Renderers.DocumentRenderers;
using Melville.Pdf.Model.Renderers.FontRenderings.DefaultFonts;
using Melville.Pdf.SkiaSharp;

namespace Melville.Pdf.FuzzTest;

public static class ParseFile
{
    public static async ValueTask Do(string fileName)
    {
        ClearLine();
        Console.WriteLine($"{fileName}");
        await using var stream = File.OpenRead(fileName);
        await Do(stream, fileName); 
    }

    private static async ValueTask Do(FileStream source, string path)
    {
        try
        {
            using var doc = await DocumentRendererFactory.CreateRendererAsync(
                await PdfDocument.ReadAsync(source), WindowsDefaultFonts.Instance);
            int completed = 0;
            object mutex = new();
            await Parallel.ForEachAsync(Enumerable.Range(1, doc.TotalPages), async (i,_) =>
            {
                await RenderPage(doc, i);
                lock (mutex)
                {
                    if (++completed % 10 == 0)
                    {
                        ClearLine();
                        Console.Write($" {completed}/{doc.TotalPages}");
                    }
                }
            });
        }
        catch (Exception e)
        {
            ClearLine();
            OutputException(e, path);
        }
    }

    private static async Task RenderPage(DocumentRenderer doc, int page)
    {
        try
        {
            await RenderWithSkia.ToSurfaceAsync(doc, page);
        }
        catch (Exception e)
        {
            OutputException(e, $"Page {page}");
        }
    }
    static void ClearLine(){
        Console.SetCursorPosition(0, Console.CursorTop);
        Console.Write(new string(' ', Console.WindowWidth)); 
        Console.SetCursorPosition(0, Console.CursorTop);
    }


    private static void OutputException(Exception e, string page)
    {
        Console.WriteLine(page);
        Console.WriteLine("    " +e.Message);
    }
}