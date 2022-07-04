using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using Diagnostics.Tracing.StackSources;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Filters;
using Melville.Pdf.LowLevel.Filters.Jbig2Filter;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Writers;
using Melville.Pdf.Model.Documents;
using Melville.Pdf.Model.Renderers;
using Melville.Pdf.Model.Renderers.FontRenderings.DefaultFonts;
using Melville.Pdf.ReferenceDocuments.Graphics.Images.JBig;
using Melville.Pdf.SkiaSharp;
using Melville.Pdf.Wpf.Rendering;

namespace Performance.Playground.Rendering;

[MemoryDiagnoser()]
public class JbigParsing
{
    [Benchmark]
    public async Task ParseJBig()
    {
        var fact = new JbigAllPageReader();
        await fact.ProcessFileBitsAsync(JBigSampleStreams.Get("042_24")!);
    }
}

[MemoryDiagnoser()]
public class CcittParsing
{
    [Benchmark]
    public async Task RenderSkia()
    {
        AwaitConfig.ResumeOnCalledThread(false);
        using var dr = await LoadDocument();
        await RenderWithSkia.ToSurface(dr, 9); 
    }
    [Benchmark]
    public void RenderWpf()
    {
        AsyncPump.Run(async () =>
        {
            AwaitConfig.ResumeOnCalledThread(true);
            using var dr = await LoadDocument();
            await new RenderToDrawingGroup(dr, 9).RenderToDrawingImage();
        });
    }

    private static async Task<DocumentRenderer> LoadDocument()
    {
        var file = File.Open(@"C:\Users\jmelv\Documents\Scratch\PDF torture test\CHAM_4_Book_Set.pdf", FileMode.Open);
        var dr = await DocumentRendererFactory.CreateRendererAsync(
            await PdfDocument.ReadAsync(file), WindowsDefaultFonts.Instance);
        return dr;
    }

}