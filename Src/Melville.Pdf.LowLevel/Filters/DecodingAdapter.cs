using System;
using System.Buffers;
using System.IO;
using System.IO.Pipelines;
using System.Threading;
using System.Threading.Tasks;

namespace Melville.Pdf.LowLevel.Filters
{
    public abstract class DecodingAdapter : Stream
    {
        private PipeReader source;
        private bool doneReading = false;
        
        protected DecodingAdapter(PipeReader source)
        {
            this.source = source;
        }

        public override IAsyncResult BeginWrite(
            byte[] buffer, int offset, int count, AsyncCallback? callback, object? state) =>
            throw new NotSupportedException();

        public override void Close() => source?.Complete();

        protected override void Dispose(bool disposing) => source.Complete();

        public override ValueTask DisposeAsync() => source.CompleteAsync();

        public override void EndWrite(IAsyncResult asyncResult) =>
            throw new NotSupportedException();

        public abstract (SequencePosition SourceConsumed, int bytesWritten, bool Done) Decode(
            ref SequenceReader<byte> source, ref Span<byte> destination);
        public abstract (SequencePosition SourceConsumed, int bytesWritten, bool Done) FinalDecode(
            ref SequenceReader<byte> source, ref Span<byte> destination);

        
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

        public override int Read(byte[] buffer, int offset, int count) =>
            throw new NotSupportedException("Only Async reads are supported.");

        public override int Read(Span<byte> buffer) =>
            throw new NotSupportedException("Only Async reads are supported.");

        public override Task<int> ReadAsync(
            byte[] buffer, int offset, int count, CancellationToken cancellationToken) =>
            ReadAsync(buffer.AsMemory(offset, count), cancellationToken).AsTask();

        public async override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = new CancellationToken())
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

        public override void Write(byte[] buffer, int offset, int count)=>
            throw new NotSupportedException();

        public override void Write(ReadOnlySpan<byte> buffer)=>
            throw new NotSupportedException();

        public override Task WriteAsync(
            byte[] buffer, int offset, int count, CancellationToken cancellationToken)=>
            throw new NotSupportedException();

        public override ValueTask WriteAsync(
            ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = new CancellationToken())
            => throw new NotSupportedException();

        public override void WriteByte(byte value)=>
            throw new NotSupportedException();

        public override void Flush()=>
            throw new NotSupportedException();

        public override Task FlushAsync(CancellationToken cancellationToken)=>
            throw new NotSupportedException();

        public override long Seek(long offset, SeekOrigin origin)=>
            throw new NotSupportedException();

        public override void SetLength(long value)=>
            throw new NotSupportedException();

        public override bool CanRead => true;
        public override bool CanSeek => false;
        public override bool CanTimeout => false;
        public override bool CanWrite => true;
        public override long Length => 0;
        public override long Position { get; set; }
    }
}