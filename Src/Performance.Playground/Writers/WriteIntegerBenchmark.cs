using System;
using BenchmarkDotNet.Attributes;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.LowLevel.Writers.ObjectWriters;

namespace Performance.Playground.Writers
{
    [MemoryDiagnoser]
    public class WriteIntegerBenchmark
    {
        private int[] values;

        public WriteIntegerBenchmark()
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
                IntegerWriter.Write(span, values[i]);
            }
        }

        [Benchmark(Baseline = true)]
        public void ToStringMethod()
        {
            var span = new Span<byte>(new byte[15]);
            for (int i = 0; i < values.Length; i++)
            {
                CopyToSpan(i, ref span);
            }
        }

        private void CopyToSpan(int i, ref Span<byte> span)
        {
            Span<Char> chars = stackalloc char[20];
            values[i].TryFormat(chars, out var written);
            for (int j = 0; j < written; j++)
            {
                span[j] = (byte) chars[j];
            }
        }
    }
}