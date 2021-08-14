using System;
using System.Buffers;
using System.IO;
using System.IO.Pipelines;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.LowLevel.Filters.RunLengthEncodeFilters
{
    public static class RleConstants
    {
        public const byte EndOfStream = 128;

        //It turns out this operation is symmetric, it will convert a length to a control byte
        // or a controlbyte to a length
        public static int RepeatedRunLength(int controlByte) => 257 - controlByte;
    }
    public class RunLengthEncoder : IStreamEncoder
    {
        public ValueTask<Stream> Encode(Stream data, PdfObject? parameters)
        {
            return new(new MinimumReadSizeFilter(
                new RunLengthEncodeWrapper(PipeReader.Create(data)), 129));
        }

        private class RunLengthEncodeWrapper : ConvertingStream
        {
            public RunLengthEncodeWrapper(PipeReader source) : base(source)
            {
            }

            protected override (SequencePosition SourceConsumed, int bytesWritten, bool Done) 
                Convert(ref SequenceReader<byte> source, ref Span<byte> destination)
            {
                return new RleEncoderEngine(source, destination).Convert(false);
            }

            protected override (SequencePosition SourceConsumed, int bytesWritten, bool Done) FinalConvert(
                ref SequenceReader<byte> source, ref Span<byte> destination)
            {
                return new RleEncoderEngine(source, destination).Convert(true);
            }
        }
    }
}