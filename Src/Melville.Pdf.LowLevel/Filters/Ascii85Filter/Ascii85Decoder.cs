using System;
using System.Buffers;
using System.Data.SqlTypes;
using System.IO;
using System.IO.Pipelines;
using System.Threading;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Parsing.ObjectParsers;

namespace Melville.Pdf.LowLevel.Filters.Ascii85Filter
{
    public class Ascii85Decoder: IDecoder
    {
        public Stream WrapStream(Stream input, PdfObject parameter) => 
            new Ascii85Adapter(input.AsPipeReader());

        private class Ascii85Adapter: DecodingAdapter
        {
            public Ascii85Adapter(PipeReader source) : base(source)
            {
            }

            public override (SequencePosition SourceConsumed, int bytesWritten, bool Done) 
                Decode(ref SequenceReader<byte> source, ref Span<byte> destination)
            {
                var destPosition = 0;
                while (true)
                {
                    var lastPosition = source.Position;
                    if (destination.Length - destPosition < 4) 
                        return (lastPosition, destPosition, false);
                    if (!source.TryReadNonWhitespace(out byte b1)) 
                        return (lastPosition, destPosition, false);
                    if (b1 == terminatingChar) return (source.Position, destPosition, true);
                    if (b1 == (byte) 'z')
                    {
                        destPosition = WriteQuad(destination, 0, 4, destPosition);
                    }
                    else
                    {
                        if (!(source.TryReadNonWhitespace(out var b2) && source.TryReadNonWhitespace(out var b3) &&
                              source.TryReadNonWhitespace(out var b4) && source.TryReadNonWhitespace(out var b5)))
                            return (lastPosition, destPosition, false);
                        bool isDone;
                        (destPosition, isDone) = HandleQuintuple(b1,b2,b3,b4,b5, ref destination, destPosition);
                        if (isDone) return (source.Position, destPosition, true);
                    }
                }
            }

            private (int, bool) HandleQuintuple(byte b1, byte b2, byte b3, byte b4, byte b5,
                ref Span<byte> destination, int destPosition) => (b1, b2, b3, b4, b5) switch
                {
                    (_, terminatingChar, _, _, _) => 
                        (WriteQuad(destination, ComputeQuad(b1), 0, destPosition), true),
                    (_, _, terminatingChar, _, _) => 
                        (WriteQuad(destination, ComputeQuad(b1, b2), 1, destPosition), true),
                    (_, _, _, terminatingChar, _) => 
                        (WriteQuad(destination, ComputeQuad(b1, b2, b3), 2, destPosition), true),
                    (_, _, _, _, terminatingChar) => 
                        (WriteQuad(destination, ComputeQuad(b1, b2, b3, b4), 3, destPosition), true),
                    _ => (WriteQuad(destination, ComputeQuad(b1, b2, b3, b4, b5), 4, destPosition), false)
                };

            private const byte firstChar = (byte)'!';
            private const byte incompleteGroupPadding = (byte)'u';
            private const byte terminatingChar = (byte)'~';

            private uint ComputeQuad(
                byte b1 = incompleteGroupPadding, 
                byte b2 = incompleteGroupPadding, 
                byte b3 = incompleteGroupPadding, 
                byte b4 = incompleteGroupPadding, 
                byte b5 = incompleteGroupPadding)
            {
                uint ret = (uint) b1 - firstChar;
                ret *= 85;
                ret += (uint) b2 - firstChar;
                ret *= 85;
                ret += (uint) b3 - firstChar;
                ret *= 85;
                ret += (uint) b4 - firstChar;
                ret *= 85;
                ret += (uint) b5 - firstChar;
                return ret;

            }

            private int WriteQuad(Span<byte> destination, uint data, int toWrite, int destPosition)
            {
                for (int i = toWrite; i < 4; i++)
                {
                    data >>= 8;
                }

                for (int i = toWrite-1; i >= 0; i--)
                {
                    destination[destPosition + i] = (byte)(data & 0xFF);
                    data >>= 8;
                }

                return destPosition + toWrite;
            }

            public override (SequencePosition SourceConsumed, int bytesWritten, bool Done) FinalDecode(ref SequenceReader<byte> source,
                ref Span<byte> destination)
            {
                if (!source.TryReadNonWhitespace(out var b1) || b1 == terminatingChar) 
                    return (source.Sequence.Start, 0, false);
                if (!source.TryReadNonWhitespace(out var b2) || b2 == terminatingChar)
                {
                    throw new InvalidDataException("Single character group in a Ascii85 stream");
                }

                if (!source.TryReadNonWhitespace(out var b3))
                {
                    b3 = terminatingChar;
                }
                if (!source.TryReadNonWhitespace(out var b4))
                {
                    b4 = terminatingChar;
                }
                var (pos, done) = HandleQuintuple(b1, b2, b3, b4, terminatingChar, ref destination, 0); 
                return (source.Position, pos, done);
            }
        }
    }
}