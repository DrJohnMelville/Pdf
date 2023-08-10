using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Melville.Pdf.Model.Documents;
using Melville.Pdf.Model.Renderers.DocumentRenderers;
using Melville.Pdf.Model.Renderers.FontRenderings.DefaultFonts;
using Melville.Pdf.SkiaSharp;

namespace Melville.Pdf.FuzzTest;

public static class NotParallel
{
    public static async Task ForEachAsync<TSource>(IEnumerable<TSource> source,
        Func<TSource, CancellationToken, ValueTask> body)
    {
        foreach (var item in source)
        {
            await body(item, CancellationToken.None);
        }
    }
}

public static class ParseFile
{
    public static async ValueTask DoAsync(string fileName, ExceptionLogger logger)
    {
        ClearLine();
        await using var stream = File.OpenRead(fileName);
        await DoAsync(stream, fileName, logger); 
    }

    private static async ValueTask DoAsync(
        FileStream source, string path, ExceptionLogger exceptionLogger)
    {
        string fileName = Max60(Path.GetFileNameWithoutExtension(path));
        ReportProgress(fileName, 0, 0);
        try
        {
            using var doc = await DocumentRendererFactory.CreateRendererAsync(
                await PdfDocument.ReadAsync(source), WindowsDefaultFonts.Instance);
            int completed = 0;
            object mutex = new();
            var cts = new CancellationTokenSource();
            await Parallel.ForEachAsync(Enumerable.Range(1, doc.TotalPages), cts.Token, async (i,_) =>
            {
                await RenderPageAsync(doc, i, path, exceptionLogger);
                lock (mutex)
                {
                    if (Console.KeyAvailable)
                    {
                        while (Console.KeyAvailable)
                        {
                            Console.ReadKey();
                        }

                        cts.Cancel();
                        return;
                    }
                    if (++completed % 10 == 0)
                    {
                        ReportProgress(fileName, completed, doc.TotalPages);
                    }
                }
            });
        }
        catch (Exception e)
        {
            exceptionLogger.Log(path, -1, e.Message);
        }
    }

    private static string Max60(string str) =>
        str.Length > 60 ? str[..60] : str;
    
    private static void ReportProgress(string fileName, int completed, int totalPages)
    {
        ClearLine();
        Console.Write($"{fileName} ({completed}/{totalPages})");
    }


    private static async Task RenderPageAsync(DocumentRenderer doc, int page, string path,
        ExceptionLogger exceptionLogger)
    {
        try
        {
            await RenderWithSkia.ToSurfaceAsync(doc, 1);
        }
        catch (Exception e)
        {
            exceptionLogger.Log(path, page, e.Message);
        }
    }
    static void ClearLine(){
        Console.SetCursorPosition(0, Console.CursorTop);
        Console.Write(new string(' ', Console.WindowWidth)); 
        Console.SetCursorPosition(0, Console.CursorTop);
    }
}