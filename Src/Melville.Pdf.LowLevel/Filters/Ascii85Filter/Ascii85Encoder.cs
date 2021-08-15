using System;
using System.Buffers;
using System.IO;
using System.IO.Pipelines;
using System.Text;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Filters.StreamFilters;
using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.LowLevel.Filters.Ascii85Filter
{
    public class Ascii85Encoder : IStreamEncoder, IStreamFilterDefinition
    {
        public ValueTask<Stream> Encode(Stream data, PdfObject? parameters) =>
            new(ReadingFilterStream.Wrap(data, this));

        public int MinWriteSize => 7;

        public (SequencePosition SourceConsumed, int bytesWritten, bool Done)
            Convert(ref SequenceReader<byte> source, ref Span<byte> destination)
        {
            int i;
            for (i = 0; i + 4 < destination.Length && TryGetQuad(ref source, out uint quad); i += 5)
            {
                if (quad == 0)
                {
                    destination[i] = (byte)'z';
                    i -= 4; // we need to step back to compensate for the shorter code;
                }
                else
                {
                    EncodeValue(destination.Slice(i, 5), quad, 5);
                }
            }

            return (source.Position, Math.Min(destination.Length, i), false);
        }

        private bool TryGetQuad(ref SequenceReader<byte> source, out uint readVal)
        {
            if (source.Remaining < 4)
            {
                readVal = 0;
                return false;
            }

            readVal = ReadBigEndianUInt(source.UnreadSpan);
            source.Advance(4);
            return true;
        }

        private static uint ReadBigEndianUInt(ReadOnlySpan<byte> span) =>
            (uint)((span[0] << 24) | (span[1] << 16) | (span[2] << 8) | span[3]);

        private static void EncodeValue(Span<byte> buffer, uint value, int bytesToWrite)
        {
            for (int index = 4; index >= 0; index--)
            {
                if (index < bytesToWrite)
                    buffer[index] = (byte)((value % 85) + Ascii85Constants.FirstChar);
                value /= 85;
            }
        }

        public (SequencePosition SourceConsumed, int bytesWritten, bool Done)
            FinalConvert(ref SequenceReader<byte> source,
                ref Span<byte> destination)
        {
            if (destination.Length < 7) return (source.Position, 0, false);
            var (finalBytes, finalQuad) = ReadPartialQuad(source);
            var lastCodeLen = EncodePartialValue(destination, finalBytes, finalQuad);
            AddStreamTerminator(ref destination, lastCodeLen);
            return (source.Position, lastCodeLen + 2, true);
        }

        private static (int count, uint value) ReadPartialQuad(SequenceReader<byte> source)
        {
            var count = 0;
            uint value = 0;
            for (int i = 0; i < 4; i++)
            {
                value <<= 8;
                if (source.TryRead(out byte val))
                {
                    count++;
                    value |= val;
                }
            }

            return (count, value);
        }

        private static int EncodePartialValue(Span<byte> destination, int count, uint value)
        {
            int lastCodeLen = 0;
            if (count > 0)
            {
                lastCodeLen = count + 1;
                EncodeValue(destination, value, lastCodeLen);
            }

            return lastCodeLen;
        }

        private static void AddStreamTerminator(ref Span<byte> destination, int lastCodeLen)
        {
            destination[lastCodeLen] = Ascii85Constants.FirstTerminatingChar;
            destination[lastCodeLen + 1] = Ascii85Constants.SecondTerminatingChar;
        }
    }
}