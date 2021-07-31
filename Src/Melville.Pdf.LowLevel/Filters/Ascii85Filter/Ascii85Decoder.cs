using System;
using System.Buffers;
using System.IO;
using System.IO.Pipelines;
using Melville.Pdf.LowLevel.Model.Objects;
using Microsoft.VisualBasic.CompilerServices;

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
                    if (destination.Length - destPosition < 4) return (lastPosition, destPosition, false);
                    if (!source.TryRead(out byte b1)) return (lastPosition, destPosition, false);
                    if (b1 == (byte) 'z')
                    {
                        destPosition = WriteQuad(destination, 0, 4, destPosition);
                    }
                    else
                    {
                        if (!(source.TryRead(out var b2) && source.TryRead(out var b3) &&
                              source.TryRead(out var b4) && source.TryRead(out var b5)))
                            return (lastPosition, destPosition, false);
                        destPosition = WriteQuad(destination, ComputeQuad(b1, b2, b3, b4, b5), 4, destPosition);
                    }

                }
            }

            const byte firstChar = (byte)'!';
            private uint ComputeQuad(byte b1, byte b2, byte b3, byte b4, byte b5)
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
                return (source.Position, 0, false);
            }
        }
    }
}