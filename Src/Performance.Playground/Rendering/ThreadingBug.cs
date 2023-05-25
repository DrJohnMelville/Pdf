using Melville.Pdf.Model.Documents;
using Melville.Pdf.Model.Renderers.DocumentRenderers;
using Melville.Pdf.Model.Renderers.FontRenderings.DefaultFonts;
using System.Linq;
using System;
using System.IO;
using System.Threading.Tasks;
using Melville.Parsing.Streams;
using Melville.Pdf.SkiaSharp;
using Melville.SharpFont.Internal;

namespace Performance.Playground.Rendering;

public class ThreadingBug
{
    public async Task ReadMultiAsync()
    {
            using var doc = await DocumentRendererFactory.CreateRendererAsync(
                await PdfDocument.ReadAsync(
                    ReadFile()), WindowsDefaultFonts.Instance);

            await Parallel.ForEachAsync(Enumerable.Range(1, 1000), async (i, _) =>
            {
                await RenderWithSkia.ToSurfaceAsync(doc, 1);
            });
    }

    private static Stream ReadFile()
    {
        using var doc= File.Open(@"C:\Users\jmelv\Documents\Scratch\1Page.pdf",
            FileMode.Open);
        var ret = new MultiBufferStream();
        doc.CopyTo(ret);
        return ret.CreateReader();
    }
}