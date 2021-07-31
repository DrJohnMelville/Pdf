using System;

namespace Melville.Pdf.LowLevel.Filters
{
    public ref struct OutputBuffer
    {
        private readonly byte[] bytes;
        private int position;

        public OutputBuffer(byte[] bytes, int position = 0)
        {
            this.bytes = bytes;
            this.position = position;
        }

        public void Append(char c) => Append((byte) c);
        public void Append(byte b) => bytes[position++] = b;
        public void Set(byte b, int offset) => bytes[position + offset] = b;
        public void Set(uint b, int offset) => Set((byte) b, offset);
        public void Increment(int delta) => position += delta;


        public byte[] Result()
        {
            var ret = bytes;
            if (position != ret.Length)
            {
                Array.Resize(ref ret, position);
            }

            return ret;
        }
    }
}