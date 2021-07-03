using System;
using System.Buffers;

namespace Melville.Pdf.LowLevel.Parsing
{
    public static class SequenceReaderExtensions
    {
        public static bool TryAdvance<T>(this ref SequenceReader<T> input, int positions) 
            where T:unmanaged, IEquatable<T>
        {
            if (input.Remaining < positions) return false;
            input.Advance(positions);
            return true;
        }
    }
}