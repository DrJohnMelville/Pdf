using System;
using System.Buffers;
using System.IO;
using System.IO.Pipelines;
using System.Threading;
using System.Threading.Tasks;

namespace Melville.Pdf.LowLevel.Filters
{
    public abstract class DecodingAdapter : SequentialReadFilterStream
    {
        private PipeReader source;
        private bool doneReading = false;
        
        protected DecodingAdapter(PipeReader source)
        {
            this.source = source;
        }
        public override void Close() => source.Complete();

        protected override void Dispose(bool disposing) => source.Complete();

        public override ValueTask DisposeAsync() => source.CompleteAsync();

        public abstract (SequencePosition SourceConsumed, int bytesWritten, bool Done) Decode(
            ref SequenceReader<byte> source, ref Span<byte> destination);
        public abstract (SequencePosition SourceConsumed, int bytesWritten, bool Done) FinalDecode(
            ref SequenceReader<byte> source, ref Span<byte> destination);


        public override async ValueTask<int> ReadAsync(
            Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            if (buffer.Length < 1 || doneReading ) return 0;
            var ret = 0;
            do
            {
                var result = await source.ReadAsync();
                ret = HandleResult(buffer.Span, result);
                if (result.IsCompleted) return ret;
            } while (ret < 1);
            return ret;
        }

        private int HandleResult(Span<byte> buffer, ReadResult result)
        {
            if (result.IsCanceled || doneReading) return 0;
            var reader = new SequenceReader<byte>(result.Buffer);
            var (finalPos, bytesWritten, done) = Decode(ref reader, ref buffer);
            if (result.IsCompleted)
            {
                (finalPos, bytesWritten, done) = HandleFinalDecode(buffer, result, finalPos, bytesWritten);
            }
            source.AdvanceTo(finalPos, result.Buffer.End);
            if (done) doneReading = true;
            Position += bytesWritten;
            return bytesWritten;
        }

        private (SequencePosition finalPos, int bytesWritten, bool done) HandleFinalDecode(Span<byte> buffer, ReadResult result,
            SequencePosition finalPos, int bytesWritten)
        {
            bool done;
            int extrBytes;
            var r2 = new SequenceReader<byte>(result.Buffer.Slice(finalPos));
            var remaining = buffer.Slice(bytesWritten);
            (finalPos, extrBytes, done) = FinalDecode(ref r2, ref remaining);
            bytesWritten += extrBytes;
            return (finalPos, bytesWritten, done);
        }
    }
}