using System;
using System.Buffers;
using System.Diagnostics;
using System.IO;
using System.IO.Pipelines;
using System.Threading.Tasks;
using System.Threading.Tasks.Sources;
using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.LowLevel.Filters.RunLengthEncodeFilters
{
    public class RunLengthDecoder: IDecoder
    {
        public ValueTask<Stream> WrapStreamAsync(Stream input, PdfObject parameter) =>
            new(
                new MinimumReadSizeFilter(
                    new RunLengthDecodeWrapper(PipeReader.Create(input)), 128));
    }

    public class RunLengthDecodeWrapper : ConvertingStream
    {
        public RunLengthDecodeWrapper(PipeReader source) : base(source)
        {
        }

        protected override (SequencePosition SourceConsumed, int bytesWritten, bool Done) Convert(
            ref SequenceReader<byte> source, ref Span<byte> destination)
        {
            return new RleDecoderEngine(source, destination).Convert();
        }
    }
}