using System;
using BenchmarkDotNet.Attributes;
using Melville.Pdf.LowLevel.Filters.FlateFilters;

namespace Performance.Playground.Writers
{
    public class Adler32
    {
        private byte[] data;
        public Adler32()
        {
            data = new byte[20000];
            for (int i = 0; i < 20000; i++)
            {
                data[i] = (byte) i;
            }
        }

        [Benchmark()]
        public void Fast() => new Adler32Computer(1).AddData(data);
        [Benchmark(Baseline = true)]
        public void Slow() => new Adler32Slow(1).AddData(data);

        public ref struct Adler32Slow
        {
            private ulong s1;
            private ulong s2;

            public Adler32Slow(uint priorAdler = 1)
            {
                s1 = priorAdler & 0xFFFF;
                s2 = (priorAdler >> 16) & 0xFFFF;
            }
            private const ulong AdlerBase = 65521; /* largest prime smaller than 65536 */
        
            public void AddData(Span<byte> bytes)
            {
                for (int i = 0; i < bytes.Length; i++)
                {
                    s1 += bytes[i];
                    s1 %= AdlerBase;
                    s2 += s1;
                    s2 %= AdlerBase;
                }
            }


            public uint GetHash() =>(uint) ((s2 << 16) | s1);
        }
    }
}