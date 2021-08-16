using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Filters.StreamFilters;

namespace Melville.Pdf.LowLevel.Filters.FlateFilters
{
    public class Adler32Computer
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

        public void AddData(ReadOnlySpan<byte> bytes)
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

    public class ReadAdlerStream : SequentialReadFilterStream
    {
        public uint GetHash() => computer.GetHash();
        private Adler32Computer computer = new Adler32Computer();
        private Stream source;

        public ReadAdlerStream(Stream source)
        {
            this.source = source;
        }

        public override async ValueTask<int> ReadAsync(
            Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            var ret = await source.ReadAsync(buffer, cancellationToken);
            computer.AddData(buffer.Span[..ret]);
            return ret;
        }
    }
}