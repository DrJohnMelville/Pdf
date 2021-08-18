using System;
using System.Buffers;
using System.IO;
using System.IO.Pipelines;
using System.Threading;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Filters.Ascii85Filter;

namespace Melville.Pdf.LowLevel.Filters.StreamFilters
{
    public class WritingFilterStream : Stream
    {
        private readonly Pipe source = new ();
        private readonly PipeWriter destination;
        private readonly IStreamFilterDefinition filter;

        public WritingFilterStream(Stream destination, IStreamFilterDefinition filter):
            this(PipeWriter.Create(destination), filter){}
        public WritingFilterStream(PipeWriter destination, IStreamFilterDefinition filter)
        {
            this.destination = destination;
            this.filter = filter;
        }

        public override IAsyncResult BeginRead(
            byte[] buffer, int offset, int count, AsyncCallback? callback, object? state) =>
            throw new NotSupportedException();
        
        
        protected override void Dispose(bool disposing) => DisposeAsync().GetAwaiter().GetResult();

        public override async ValueTask DisposeAsync()
        {
            await source.Writer.CompleteAsync();
            HandleMultipleWrites();
            await destination.CompleteAsync();
        }

        public override void Flush() => FlushAsync().GetAwaiter().GetResult();

        public override async Task FlushAsync(CancellationToken cancellationToken)
        {
            await source.Writer.FlushAsync();
            HandleMultipleWrites();
            await destination.FlushAsync(cancellationToken);
        }

        public override int Read(byte[] buffer, int offset, int count) => 
            throw new NotSupportedException();

        public override int Read(Span<byte> buffer)=> 
            throw new NotSupportedException();

        public override Task<int> ReadAsync(
            byte[] buffer, int offset, int count, CancellationToken cancellationToken)=> 
            throw new NotSupportedException();

        public override ValueTask<int> ReadAsync(
            Memory<byte> buffer, CancellationToken cancellationToken = default)=> 
            throw new NotSupportedException();

        public override int ReadByte()=> 
            throw new NotSupportedException();

        public override long Seek(long offset, SeekOrigin origin)=> 
            throw new NotSupportedException();

        public override void SetLength(long value)=> 
            throw new NotSupportedException();

        public override void Write(byte[] buffer, int offset, int count)=> 
            WriteAsync(buffer, offset, count).GetAwaiter().GetResult();

        public override void Write(ReadOnlySpan<byte> buffer)=> 
            throw new NotSupportedException();

        public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken) => 
            WriteAsync(buffer.AsMemory(offset, count), cancellationToken).AsTask();

        public override async ValueTask WriteAsync(
            ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
        {
            CopyDataToInoutBuffer(buffer);
            HandleMultipleWrites();
        }

        private void CopyDataToInoutBuffer(ReadOnlyMemory<byte> buffer)
        {
            var dest = source.Writer.GetSpan(buffer.Length);
            buffer.Span.CopyTo(dest);
            source.Writer.Advance(buffer.Length);
        }

        private void HandleMultipleWrites()
        {
            while (HandleWrite() > 0) /* Do nothing run the method until it fails.*/;
        }

        private bool done;
        private int HandleWrite()
        {
            if (done) return 0;
            if (!source.Reader.TryRead(out var rr)) return 0;
            var reader = new SequenceReader<byte>(rr.Buffer);
            var destBuff = destination.GetSpan(filter.MinWriteSize);
            SequencePosition pos;
            int localWritten;
            (pos, localWritten, done) = filter.Convert(ref reader, ref destBuff);
            if (localWritten == 0 && rr.IsCompleted)
            {
                (pos, localWritten, done) = filter.FinalConvert(ref reader, ref destBuff);
            } 
            source.Reader.AdvanceTo(pos);
            destination.Advance(localWritten);
            return localWritten;
        }

        public override void WriteByte(byte value)=> 
            throw new NotSupportedException();

        public override bool CanRead => false;

        public override bool CanSeek => false;

        public override bool CanTimeout => false;

        public override bool CanWrite => true;

        public override long Length => 0;

        public override long Position
        {
            get => 0;
            set => throw new NotSupportedException();
        }
    }
}