﻿using Melville.Pdf.Model.Documents;
using Melville.Pdf.Model.Renderers.DocumentRenderers;
using Melville.Pdf.Model.Renderers.FontRenderings.DefaultFonts;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using Melville.Parsing.MultiplexSources;
using Melville.Parsing.Streams;
using Melville.Pdf.SkiaSharp;

namespace Performance.Playground.Rendering;

public class ThreadingBug
{
    public async Task ReadMultiAsync()
    {
        await using var readFile = ReadFile();
        using var doc = await DocumentRendererFactory.CreateRendererAsync(
                await PdfDocument.ReadAsync(
                    readFile), WindowsDefaultFonts.Instance);

            await Parallel.ForEachAsync(Enumerable.Range(1, 1000), async (i, _) =>
            {
                await RenderWithSkia.ToSurfaceAsync(doc, 1);
            });
    }

    private static Stream ReadFile()
    {
        using var doc= File.Open(@"C:\Users\jmelv\Documents\Scratch\1Page.pdf",
            FileMode.Open);
        using var ret = WritableBuffer.Create();
        using var writer = ret.WritingStream();
        doc.CopyTo(writer);
        return ret.ReadFrom(0);
    }
}