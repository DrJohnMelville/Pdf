using System.Buffers;
using System.Numerics;

namespace Melville.Parsing.SequenceReaders
{
    internal readonly struct GenericIntReader<T> where T:IBinaryInteger<T>
    {
        private static readonly int byteCount = T.Zero.GetByteCount();

        public static bool TryReadBigEndian(ref SequenceReader<byte> reader, out T value)
        {
            if (reader.Remaining < byteCount)
            {
                value = T.Zero;
                return false;
            }

            if (reader.UnreadSpan.Length >= byteCount)
                ReadFast(reader.UnreadSpan, out value);
            else
                ReadSlow(ref reader, out value);

            reader.Advance(byteCount);
            return true;
        }

        private static void ReadSlow(ref SequenceReader<byte> reader, out T value)
        {
            Span<byte> span = stackalloc byte[byteCount];
            reader.TryCopyTo(span);
            ReadFast(span, out value);
        }

        private static void ReadFast(in ReadOnlySpan<byte> span, out T value)
        {
            value = T.Zero;
            for (int i = 0; i < byteCount; i++)
            {
                value <<= 8;
                value |= T.CreateTruncating(span[i]);
            }
        }
    }
}