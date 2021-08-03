using System;

namespace Melville.Pdf.LowLevel.Filters.FlateFilters
{
    public ref struct Adler32Computer
    {
        private ulong s1;
        private ulong s2;

        public Adler32Computer(uint priorAdler = 1)
        {
            s1 = priorAdler & 0xFFFF;
            s2 = (priorAdler >> 16) & 0xFFFF;
        }
        private const ulong BiggestUintPrime = 65521; /* largest prime smaller than 65536 */
        private const int minIterationstoFillUint = 5552;

        public void AddData(Span<byte> bytes)
        {
            for (int i = 0; i < bytes.Length;)
            {
                var limit = Math.Min(bytes.Length, i + minIterationstoFillUint);
                for (; i < limit; i++)
                {
                    s1 += bytes[i];
                    s2 += s1;
                }
                s1 %= BiggestUintPrime;
                s2 %= BiggestUintPrime;
            }
        }
        

        public uint GetHash() =>(uint) ((s2 << 16) | s1);
    }
}