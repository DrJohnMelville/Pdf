using System.IO;
using System.Threading.Tasks;
using System.Windows.Threading;
using BenchmarkDotNet.Attributes;
using JetBrains.Profiler.Api;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Parsing.FileParsers;
using Melville.Pdf.Model.Documents;
using Melville.Pdf.Model.Renderers;
using Melville.Pdf.Model.Renderers.FontRenderings.DefaultFonts;
using Melville.Pdf.SkiaSharp;
using Melville.Pdf.Wpf.Rendering;

namespace Performance.Playground.Rendering
{
[MemoryDiagnoser()]
    public class FontRenderingPerf
    {
        [Benchmark]
        public async Task RenderSkia()
        {
            AwaitConfig.ResumeOnCalledThread(false);
            using var dr = await LoadDocument();
            MeasureProfiler.StartCollectingData(); 
            await RenderWithSkia.ToSurface(dr, 1); 
            MeasureProfiler.StopCollectingData();
        }
        [Benchmark]
        public void RenderWpf()
        {
          AsyncPump.Run(async () =>
          {
            AwaitConfig.ResumeOnCalledThread(true);
            using var dr = await LoadDocument();
            MeasureProfiler.StartCollectingData();
            await new RenderToDrawingGroup(dr, 1).RenderToDrawingImage();
            MeasureProfiler.StopCollectingData();
          });
        }

        private static async Task<DocumentRenderer> LoadDocument()
        {
            var file = File.Open(@"C:\Users\jmelv\Documents\Scratch\PDF torture test\Adalms colposcopy for sexual assault 2001.PDF", FileMode.Open);
            var dr = await DocumentRendererFactory.CreateRendererAsync(
                await PdfDocument.ReadAsync(file), new WindowsDefaultFonts());
            return dr;
        }
    }
}