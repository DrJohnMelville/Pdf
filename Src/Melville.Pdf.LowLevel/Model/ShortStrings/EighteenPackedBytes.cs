using System;
using System.Diagnostics;

namespace Melville.Pdf.LowLevel.Model.ShortStrings
{
    internal readonly struct EighteenPackedBytes: IPackedBytes
    {
        private readonly NinePackedBytes first;
        private readonly NinePackedBytes second;

        public EighteenPackedBytes(in ReadOnlySpan<byte> data)
        {
            Debug.Assert(IsValidLength(data));
            first = new NinePackedBytes(data[..9]);
            second = new NinePackedBytes(data[9..]);
        }

        private static bool IsValidLength(in ReadOnlySpan<byte> data) => 
            data.Length is > 9 and < 19;

        public int Length() => first.Length() + second.Length();

        public bool SameAs(in ReadOnlySpan<byte> other) =>
            IsValidLength(other) &&
            first.SameAs(other[..9]) &&
            second.SameAs(other[9..]);

        public void Fill(in Span<byte> target)
        {
            first.Fill(target[..9]);
            second.Fill(target[9..]);
        }

        public void Fill(in Span<char> target)
        {
            first.Fill(target[..9]);
            second.Fill(target[9..]);
        }

        public void AddToHash(ref FnvComputer hash)
        {
            first.AddToHash(ref hash);
            second.AddToHash(ref hash);
        }
    }
}