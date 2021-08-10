using System;
using System.IO.Pipelines;
using System.Threading.Tasks;

namespace Melville.Pdf.LowLevel.Filters.LzwFilter
{
    public class BitReader: IDisposable
    {
        private readonly PipeReader source;

        public BitReader(PipeReader source)
        {
            this.source = source;
        }

        private byte residue;
        private int bitsRemaining;
        public async ValueTask<int?> TryRead(int bits)
        {
            if (!await TryReadByte()) return null;
            if (bitsRemaining >= bits)
            {
                return CopyLowBits(bits);
            }

            var bitsNeeded = CopyUpperBits(bits, out var firstPart);
            var lastPart = await TryRead(bitsNeeded);
            return firstPart | lastPart;
        }

        private int CopyLowBits(int bits)
        {
            var ret = BitUtilities.Mask(bits) & residue >> (bitsRemaining - bits);
            bitsRemaining -= bits;
            return ret;
        }

        private int CopyUpperBits(int bits, out int firstPart)
        {
            var bitsNeeded = bits - bitsRemaining;
            firstPart = (residue & BitUtilities.Mask(bitsRemaining)) << bitsNeeded;
            bitsRemaining = 0;
            return bitsNeeded;
        }

        private async ValueTask<bool> TryReadByte()
        {
            if (bitsRemaining > 0) return true;
            var readResult = await source.ReadAsync();
            if (readResult.IsCompleted && readResult.Buffer.Length == 0) return false;
            residue = readResult.Buffer.First.Span[0];
            bitsRemaining = 8;
            source.AdvanceTo(readResult.Buffer.GetPosition(1));
            return true;
        }

        public void Dispose() => source.Complete();
    }
}