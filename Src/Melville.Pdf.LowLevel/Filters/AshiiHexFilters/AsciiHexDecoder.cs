using System;
using System.Buffers;
using System.IO;
using System.IO.Pipelines;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Parsing.ObjectParsers;

namespace Melville.Pdf.LowLevel.Filters.AshiiHexFilters
{
    public class AsciiHexDecoder : IDecoder
    {
        public Stream WrapStream(Stream input, PdfObject parameter)
        {
            return new AsciiHexAdapter(PipeReader.Create(input, new StreamPipeReaderOptions()));
        }
        
        private class AsciiHexAdapter: DecodingAdapter
        {
            public AsciiHexAdapter(PipeReader source) : base(source)
            {
            }

            public override (SequencePosition SourceConsumed, int bytesWritten, bool Done) 
                Decode(ref SequenceReader<byte> source, ref Span<byte> destination)
            {
                int position = 0;
                bool done = false;
                SequencePosition consumed = source.Sequence.Start;
                while (position < destination.Length&&
                       GetNonWhiteSpaceChar(ref source, out var highByte) && 
                       GetNonWhiteSpaceChar(ref source, out var lowByte))
                {
                    if (highByte == (byte) '>') return (consumed, position, true);
                    destination[position++] = HexMath.ByteFromHexCharPair(highByte, lowByte);
                    consumed = source.Position;
                    if (lowByte == (byte) '>') return (consumed, position, true);
                }

                return (consumed, position, false);

            }

            private static bool GetNonWhiteSpaceChar(ref SequenceReader<byte> source, out byte item)
            {
                while (true)
                {
                    if (!source.TryRead(out item)) return false;
                    if (CharClassifier.Classify(item) != CharacterClass.White)
                        return true;
                }
            }
        }
    }
}