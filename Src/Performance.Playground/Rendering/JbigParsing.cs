using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using Melville.Pdf.LowLevel.Filters.Jbig2Filter;
using Melville.Pdf.ReferenceDocuments.Graphics.Images.JBig;

namespace Performance.Playground.Rendering;

[MemoryDiagnoser()]
public class JbigParsing
{
    [Benchmark]
    public async Task ParseJBig()
    {
        var fact = new JbigAllPageReader();
        await fact.ProcessFileBitsAsync(JBigSampleStreams.Get("200-4-45")!);
    }
}