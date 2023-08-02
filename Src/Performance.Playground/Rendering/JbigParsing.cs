using System;
using System.IO;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using Melville.JBig2;
using Melville.JBig2.BinaryBitmaps;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Filters.Jbig2Filter;
using Melville.Pdf.Model.Documents;
using Melville.Pdf.Model.Renderers.DocumentRenderers;
using Melville.Pdf.Model.Renderers.FontRenderings.DefaultFonts;
using Melville.Pdf.ReferenceDocuments.Graphics.Images.JBig;
using Melville.Pdf.SkiaSharp;
using Melville.Pdf.Wpf.Rendering;

namespace Performance.Playground.Rendering;

[MemoryDiagnoser()]
public class JbigParsing
{
    [Benchmark]
    public async Task ParseJBigAsync()
    {
        var fact = new JbigAllPageReader();
        await fact.ProcessFileBitsAsync(JBigSampleStreams.Get("042_24")!);
        var page = fact.GetPage(1);
        var (ary, _) = page.ColumnLocation(0);
        var stream = new InvertingMemoryStream(ary, page.BufferLength());
        var buffer = new byte[4096];
        while ((await stream.ReadAsync(buffer.AsMemory())) > 0)
        {
        }
    }
}

[MemoryDiagnoser()]
public class CcittParsing
{
    [Benchmark]
    public async Task RenderSkiaAsync()
    {
        AwaitConfig.ResumeOnCalledThread(false);
        using var dr = await LoadDocumentAsync();
        await RenderWithSkia.ToSurfaceAsync(dr, 9); 
    }
    [Benchmark]
    public void RenderWpf()
    {
        AsyncPump.Run(async () =>
        {
            AwaitConfig.ResumeOnCalledThread(true);
            using var dr = await LoadDocumentAsync();
            await new RenderToDrawingGroup(dr, 9).RenderToDrawingImageAsync();
        });
    }

    private static async Task<DocumentRenderer> LoadDocumentAsync()
    {
        var file = File.Open(@"C:\Users\jmelv\Documents\Scratch\PDF torture test\CHAM_4_Book_Set.pdf", FileMode.Open);
        var dr = await DocumentRendererFactory.CreateRendererAsync(
            await PdfDocument.ReadAsync(file), WindowsDefaultFonts.Instance);
        return dr;
    }

}