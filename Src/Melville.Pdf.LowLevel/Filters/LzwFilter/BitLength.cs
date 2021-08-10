using System;

namespace Melville.Pdf.LowLevel.Filters.LzwFilter
{
    public readonly struct BitLength
    {
        public int Length { get; }
        private readonly int nextIncrement;

        public BitLength(int bits)
        {
            if (bits > 12) throw new ArgumentException("Too big");
            Length = bits;
            nextIncrement = (1 << bits)-1;
        }

        public BitLength CheckBitLength(int next)
        {
            return (next >= nextIncrement) ? new BitLength(Length + 1) : this;
        }
    }
}