using System;
using System.Buffers;
using System.IO;
using System.IO.Pipelines;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Parsing.ObjectParsers;

namespace Melville.Pdf.LowLevel.Filters.AshiiHexFilters
{
    public class AsciiHexDecompressor : IDecompressor
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
                SequencePosition consumed = source.Sequence.Start;
                while (position < destination.Length&&
                       source.TryRead(out var highByte) && source.TryRead(out var lowByte))
                {
                    destination[position++] = HexMath.ByteFromHexCharPair(highByte, lowByte);
                    consumed = source.Position;
                }

                return (consumed, position, false);

            }
        }
    }
}