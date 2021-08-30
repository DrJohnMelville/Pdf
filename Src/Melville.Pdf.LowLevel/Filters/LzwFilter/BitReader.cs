using System.Buffers;

namespace Melville.Pdf.LowLevel.Filters.LzwFilter
{
    public class BitReader
    {
        private byte residue;
        private int bitsRemaining;

        public bool TryRead(int bits, ref SequenceReader<byte> input, out int value)
        {
            var ret = TryRead(bits, ref input);
            value = ret ?? 0;
            return ret.HasValue;
        }
        public int? TryRead(int bits, ref SequenceReader<byte> input)
        {
            if (bits - bitsRemaining > 8 * input.Remaining) return null; 
            if (!TryReadByte(ref input)) return null;
            if (bitsRemaining >= bits)
            {
                return CopyLowBits(bits);
            }

            var bitsNeeded = CopyUpperBits(bits, out var firstPart);
            var lastPart = TryRead(bitsNeeded, ref input) ?? 0; // this is guarenteed to succeed
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

        private bool TryReadByte(ref SequenceReader<byte> input)
        {
            if (bitsRemaining > 0) return true;
            if (!input.TryRead(out residue)) return false;
            bitsRemaining = 8;
            return true;
        }
    }
}