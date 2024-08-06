using System.IO;
using System.Threading.Tasks;
using System.Windows.Media;
using BenchmarkDotNet.Attributes;
using Melville.Parsing.AwaitConfiguration;
using Melville.Parsing.MultiplexSources;
using Melville.Parsing.Streams;
using Melville.Pdf.Model.Documents;
using Melville.Pdf.Model.Renderers.DocumentRenderers;
using Melville.Pdf.Model.Renderers.FontRenderings.DefaultFonts;
using Melville.Pdf.ReferenceDocuments.Text.TrueType;
using Melville.Pdf.SkiaSharp;
using Melville.Pdf.Wpf.Rendering;

namespace Performance.Playground.Rendering;

[MemoryDiagnoser()]
public class PageRendering
{
    MultiBufferStream data = new MultiBufferStream();
    [GlobalSetup]
    public async Task CreateStreamAsync()
    {
        await new EmbeddedTrueType().WritePdfAsync(data);
    }
    
    [Benchmark]
    public async Task RenderSkiaAsync()
    { 
        AwaitConfig.ResumeOnCalledThread(false);
        using var dr = await LoadDocumentAsync();
        await RenderWithSkia.ToSurfaceAsync(dr, 1, 2000); 
    }
    [Benchmark]
    public async Task<DrawingImage> RenderWpfAsync()
    {
            AwaitConfig.ResumeOnCalledThread(true);
            using var dr = await LoadDocumentAsync();
            return await new RenderToDrawingGroup(dr, 1).RenderToDrawingImageAsync();
    }

    private async Task<DocumentRenderer> LoadDocumentAsync() =>
        await DocumentRendererFactory.CreateRendererAsync(
            await PdfDocument.ReadAsync(((IMultiplexSource)data).ReadFrom(0)), WindowsDefaultFonts.Instance);
}