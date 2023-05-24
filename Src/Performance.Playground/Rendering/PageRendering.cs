using System.IO;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Threading;
using BenchmarkDotNet.Attributes;
using JetBrains.Profiler.Api;
using Melville.Parsing.AwaitConfiguration;
using Melville.Parsing.Streams;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Parsing.FileParsers;
using Melville.Pdf.Model;
using Melville.Pdf.Model.Documents;
using Melville.Pdf.Model.Renderers;
using Melville.Pdf.Model.Renderers.DocumentRenderers;
using Melville.Pdf.Model.Renderers.FontRenderings.DefaultFonts;
using Melville.Pdf.ReferenceDocuments.Graphics.Images.JBig;
using Melville.Pdf.ReferenceDocuments.Graphics.Patterns.Shading;
using Melville.Pdf.ReferenceDocuments.Text.TrueType;
using Melville.Pdf.SkiaSharp;
using Melville.Pdf.Wpf.Rendering;
using SkiaSharp;

namespace Performance.Playground.Rendering;

[MemoryDiagnoser()]
public class PageRendering
{
    MultiBufferStream data = new MultiBufferStream();
    [GlobalSetup]
    public async Task CreateStream()
    {
        await new EmbeddedTrueType().WritePdfAsync(data);
    }
    
    [Benchmark]
    public async Task RenderSkia()
    { 
        AwaitConfig.ResumeOnCalledThread(false);
        using var dr = await LoadDocument();
        await RenderWithSkia.ToSurfaceAsync(dr, 1, 2000); 
    }
    [Benchmark]
    public async Task<DrawingImage> RenderWpf()
    {
            AwaitConfig.ResumeOnCalledThread(true);
            using var dr = await LoadDocument();
            return await new RenderToDrawingGroup(dr, 1).RenderToDrawingImageAsync();
    }

    private async Task<DocumentRenderer> LoadDocument() =>
        await DocumentRendererFactory.CreateRendererAsync(
            await PdfDocument.ReadAsync(data.CreateReader()), WindowsDefaultFonts.Instance);
}