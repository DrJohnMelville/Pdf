using System;
using System.Buffers;
using System.IO;
using System.IO.Pipelines;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.LowLevel.Filters.LzwFilter
{
    public class LzwDecoder : IDecoder
    {
        public ValueTask<Stream> WrapStreamAsync(Stream input, PdfObject parameter) =>
            ValueTask.FromResult<Stream>(new LzwDecodeWrapper(PipeReader.Create(input)));
        
        private class LzwDecodeWrapper: DecodingAdapter
        {
            public LzwDecodeWrapper(PipeReader source) : base(source)
            {
            }

            public override (SequencePosition SourceConsumed, int bytesWritten, bool Done) Decode(ref SequenceReader<byte> source, ref Span<byte> destination)
            {
                throw new NotImplementedException();
            }

            public override (SequencePosition SourceConsumed, int bytesWritten, bool Done) FinalDecode(ref SequenceReader<byte> source,
                ref Span<byte> destination)
            {
                throw new NotImplementedException();
            }
        }
    }
}