using System.Buffers;
using System.IO;
using System.IO.Pipelines;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Filters.LzwFilter;

namespace Melville.Pdf.LowLevel.Model.Primitives.VariableBitEncoding
{
    public readonly struct BitStreamReader
    {
        private readonly PipeReader pipe;
        private readonly BitReader reader;
        private readonly int bits;

        public BitStreamReader(Stream source, int bits) : this()
        {
            pipe = PipeReader.Create(source);
            reader = new BitReader();
            this.bits = bits;
        }

        public async ValueTask<uint> NextNum()
        {
            while (true)
            {
                var span = await pipe.ReadAsync();
                if (TryRead(span.Buffer, out var ret)) return ret;
                pipe.AdvanceTo(span.Buffer.Start, span.Buffer.End);
            }
        }

        private bool TryRead(in ReadOnlySequence<byte> spanBuffer, out uint output)
        {
            var seqReader = new SequenceReader<byte>(spanBuffer);
            if (!reader.TryRead(bits, ref seqReader, out var intValue))
            {
                output = 0;
                return false;
            }
            pipe.AdvanceTo(seqReader.Position);
            output = (uint)intValue;
            return true;
        }
    }
}