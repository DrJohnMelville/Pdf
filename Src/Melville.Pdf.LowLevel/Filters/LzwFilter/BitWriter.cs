using System;

namespace Melville.Pdf.LowLevel.Filters.LzwFilter
{
    public class BitWriter
    {
        private byte residue;
        private byte spotsAvailable;
        
        public BitWriter()
        {
            residue = 0;
            spotsAvailable = 8;
        }

        public int WriteBits(uint data, int bits, in Span<byte> target) =>
            WriteBits((int)data, bits, target);
        public int WriteBits(int data, int bits, in Span<byte> target)
        {
            var position = 0;
            if (spotsAvailable == 0)
            {
                position +=  WriteCurrentByte(target);
            }
            int leftOverBits = bits - spotsAvailable;
            if (leftOverBits <= 0)
            {
                AddBottomBitsToResidue(data, bits);
                return position;
            }

            AddBottomBitsToResidue(data >> leftOverBits, spotsAvailable);
            return position + WriteBits(data, leftOverBits, target.Slice(position));
        }

        private void AddBottomBitsToResidue(int data, int bits)
        {
            residue |= (byte) ((data & BitUtilities.Mask(bits)) << (spotsAvailable - bits));
            spotsAvailable = (byte) (spotsAvailable - bits);
        }
        
        private bool NoBitsWaitingToBeWritten() => spotsAvailable > 7;

        private int WriteCurrentByte(in Span<byte> target)
        {
            WriteByte(target);
            return  1;
        }

        private void WriteByte(in Span<byte> span)
        {
            span[0] = residue;
            residue = 0;
            spotsAvailable = 8;
        }
        public int FinishWrite(in Span<byte> target) => 
            NoBitsWaitingToBeWritten() ? 0 : WriteCurrentByte(target);
    }

    public static class BitUtilities
    {
        public static byte Mask(int bits) => (byte) ((1 << bits) - 1);
    }
}