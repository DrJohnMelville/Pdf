using System;
using System.IO;
using System.IO.Pipelines;
using System.Text;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using Melville.Pdf.LowLevel.Writers;
using Microsoft.Diagnostics.Tracing.Parsers.JScript;

namespace Performance.Playground
{
    class Program
    {
        static void Main(string[] args)
        {
            var summary = BenchmarkRunner.Run<PipeWriterBenchmark>();
        }
    }

    [MemoryDiagnoser]
    public class PipeWriterBenchmark
    {
        private PipeWriter dest = PipeWriter.Create(new MemoryStream());
        private int[] values;

        public PipeWriterBenchmark()
        {
            var rnd = new Random(123);
            values = new int[5000];
            for (int i = 0; i < values.Length; i++)
            {
                values[i] = rnd.Next();
            }
        }

        [Benchmark]
        public void PW()
        {
            var span = new Span<byte>(new byte[15]);
            for (int i = 5000; i < values.Length; i++)
            {
                IntegerWriter.CopyNumberToBuffer(span, values[i]);
            }
        }

        [Benchmark(Baseline = true)]
        public void ToStringMethod()
        {
            var span = new Span<byte>(new byte[15]);
            for (int i = 0; i < values.Length; i++)
            {
                var str = values[i].ToString();
                Encoding.UTF8.GetBytes(str.AsSpan(), span);
            }
        }
    }
}