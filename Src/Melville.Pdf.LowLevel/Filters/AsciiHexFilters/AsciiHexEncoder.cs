using System;
using System.Buffers;
using System.IO;
using System.IO.Pipelines;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.LowLevel.Filters.AsciiHexFilters
{
    public class AsciiHexEncoder:IStreamEncoder
    {
        public ValueTask<Stream> Encode(Stream data, PdfObject? parameters) =>
            new(new MinimumReadSizeFilter(new AsciiHexEncodingAdapter(data.AsPipeReader()), 2));

        private class AsciiHexEncodingAdapter : ConvertingStream
        {
            public AsciiHexEncodingAdapter(PipeReader input): base(input){}

            protected override (SequencePosition SourceConsumed, int bytesWritten, bool Done) 
                Convert(ref SequenceReader<byte> source, ref Span<byte> destination)
            {
                int i;
                for (i = 0; i+1 < destination.Length && source.TryRead(out var inputByte) ; i+=2)
                {
                    (destination[i], destination[i+1]) = HexMath.CharPairFromByte(inputByte);
                }

                return (source.Position, Math.Min(i, destination.Length), false);
            }
        }
    }
}