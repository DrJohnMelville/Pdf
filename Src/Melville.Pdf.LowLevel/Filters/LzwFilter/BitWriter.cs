using System;
using System.Diagnostics;
using System.IO;

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

        public int WriteBits(int data, int bits, Span<byte> target)
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
        
        public int FinishWrite(Span<byte> target) => 
            NoBitsWaitingToBeWritten() ? 0 : WriteCurrentByte(target);

        private bool NoBitsWaitingToBeWritten() => spotsAvailable > 7;

        private int WriteCurrentByte(Span<byte> target)
        {
            WriteByte(target);
            return  1;
        }

        private void WriteByte(Span<byte> span)
        {
            span[0] = residue;
            residue = 0;
            spotsAvailable = 8;
        }
    }

    public static class BitUtilities
    {
        public static byte Mask(int bits) => (byte) ((1 << bits) - 1);
    }
}