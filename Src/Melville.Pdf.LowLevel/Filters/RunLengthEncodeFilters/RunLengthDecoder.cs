using System;
using System.Buffers;
using System.IO;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Filters.StreamFilters;
using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.LowLevel.Filters.RunLengthEncodeFilters
{
    public class RunLengthDecoder : IDecoder, IStreamFilterDefinition
    {
        public ValueTask<Stream> WrapStreamAsync(Stream input, PdfObject parameter) =>
            new(ReadingFilterStream.Wrap(input, this));

        public int MinWriteSize => 128;

        public (SequencePosition SourceConsumed, int bytesWritten, bool Done) Convert(
            ref SequenceReader<byte> source, ref Span<byte> destination)
        {
            return new RleDecoderEngine(source, destination).Convert();
        }
    }
}