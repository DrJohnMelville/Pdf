using System;
using System.IO;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using JetBrains.Profiler.Api;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Parsing.FileParsers;

namespace Performance.Playground.Rendering
{
    public class LoadCCITPerf
    {
        [Benchmark]
        public async Task RenderPage()
        {
            var file = File.Open(@"C:\Users\jmelv\Documents\Scratch\PDF torture test\CHAM_4_Book_Set.pdf", FileMode.Open);
            var lldr = await RandomAccessFileParser.Parse(file);
            var obj = lldr.Objects[(41, 0)];
            var stream = (await obj.DirectValueAsync()) as PdfStream;
            var buffer = new byte[4012];
            MeasureProfiler.StartCollectingData();
            var decode = await stream.StreamContentAsync();
            while ((await decode.ReadAsync(buffer.AsMemory())) > 0) ;
            MeasureProfiler.StopCollectingData();
        }
    }
}