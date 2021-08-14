using System;

namespace Melville.Pdf.LowLevel.Filters.LzwFilter
{
    public class BitLength
    {
        public int Length { get; private set; }
        private int nextIncrement;
        private int sizeSwitchFlavorDelta = 1;

        public BitLength(int bits, int sizeSwitchFlavorDelta)
        {
            this.sizeSwitchFlavorDelta = sizeSwitchFlavorDelta;
            SetBitLength(bits);
        }

        public void SetBitLength(int bits)
        {
            Length = bits;
            nextIncrement = (1 << bits) - sizeSwitchFlavorDelta;
        }

        public void CheckBitLength(int next)
        {
            if (next < nextIncrement) return;
            SetBitLength(Length + 1);
        }
    }
}