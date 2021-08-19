using System;
using System.IO;
using BenchmarkDotNet.Attributes;
using Melville.Pdf.LowLevel.Filters.StreamFilters;

namespace Performance.Playground.Writers
{
    [MemoryDiagnoser]
    public class MemoryStreams
    {

        [Benchmark(Baseline = true)]
        public void MemoryStreamTest() => DoBenchMark(new MemoryStream());

        [Benchmark()]
        public void MultiBufferTest() => DoBenchMark(new MultiBufferStream(1240));

        private void DoBenchMark(Stream memoryStream)
        {
            var buffer = new byte[7000];
            for (int i = 0; i < 2000; i++)
            {
                memoryStream.Write(buffer, 0, buffer.Length);
            }

            memoryStream.Seek(0, SeekOrigin.Begin);
            for (int i = 0; i < 2000; i++)
            {
                memoryStream.Read(buffer, 0, buffer.Length);
            }
        }
    }
}