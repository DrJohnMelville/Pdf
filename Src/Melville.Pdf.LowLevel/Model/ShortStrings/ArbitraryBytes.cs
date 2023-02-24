using System;

namespace Melville.Pdf.LowLevel.Model.ShortStrings
{
    internal readonly struct ArbitraryBytes : IPackedBytes
    {
        private readonly byte[] data;
        public ArbitraryBytes(in ReadOnlySpan<byte> data) => this.data = data.ToArray();

        public int Length() => data.Length;

        public bool SameAs(in ReadOnlySpan<byte> other) =>
            other.SequenceEqual(data);

        public void Fill(in Span<byte> target) => data.AsSpan().CopyTo(data);

        public void Fill(in Span<char> target)
        {
            int pos = 0;
            foreach (var datum in data)
            {
                target[pos++] = (char)datum;
            }
        }

        public void AddToHash(ref FnvComputer hash)
        {
            foreach (var datum in data)
            {
                hash.SingleHashStep(datum);
            }
        }
    }
}