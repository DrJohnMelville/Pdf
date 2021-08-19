using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Melville.Pdf.LowLevel.Filters.StreamFilters
{
    public class MultiBufferStream:Stream
    {
        private readonly MultiBuffer multiBuffer;
        
        private MultiBufferStream(MultiBuffer multiBuffer)
        {
            this.multiBuffer = multiBuffer;
        }

        public MultiBufferStream(int blockLength) : this(new MultiBuffer(blockLength))
        {
            
        }

        public MultiBufferStream(byte[] firstBuffer): this(new MultiBuffer(firstBuffer))
        {
        }

        public override int Read(Span<byte> buffer)
        {
            int bytesRead = multiBuffer.Read(Position, buffer);
            Position += bytesRead;
            return bytesRead;
        }

        public override int ReadByte()
        {
            if (Position < 0 || Position >= Length) return -1;
            Span<byte> ret = stackalloc byte[1];
            Read(ret);
            return ret[0];
        }
        public override void Write(ReadOnlySpan<byte> buffer)
        {
            multiBuffer.Write(position, buffer);
            position += buffer.Length;
        }
        
        public override void WriteByte(byte value)
        {
            Span<byte> buf = stackalloc byte[1];
            buf[0] = value;
            Write(buf);
         }

        #region Interface code to implement all of Stream's many interfaces

        public override void Flush()
        {
            // flush is not meaningful for this stream, all writes are automatic
        }

        public override int Read(byte[] buffer, int offset, int count) => 
            Read(buffer.AsSpan(offset, count));

        public override Task<int> ReadAsync(
            byte[] buffer, int offset, int count, CancellationToken cancellationToken) => 
            Task.FromResult(Read(buffer, offset, count));

        public override ValueTask<int> ReadAsync(
            Memory<byte> buffer, CancellationToken cancellationToken = new CancellationToken()) => 
            new(Read(buffer.Span));

        public override long Seek(long offset, SeekOrigin origin) => Position = offset + SeekOriginLocation(origin);

        private long SeekOriginLocation(SeekOrigin origin) => origin switch
        {
            SeekOrigin.Current => Position,
            SeekOrigin.End => Length,
            _ => 0
        };

        public override void SetLength(long value)
        {
            multiBuffer.SetLength(value);
        }

        public override void Write(byte[] buffer, int offset, int count) =>
            Write(buffer.AsSpan(offset, count));
        
        public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            return base.WriteAsync(buffer, offset, count, cancellationToken);
        }

        public override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = new CancellationToken())
        {
            return base.WriteAsync(buffer, cancellationToken);
        }
        
        public override bool CanRead => true;

        public override bool CanSeek => true;

        public override bool CanTimeout => false;

        public override bool CanWrite => true;

        #endregion

        public override long Length => multiBuffer.Length;

        private long position;
        public override long Position
        {
            get => position;
            set
            {
                multiBuffer.CheckValidPosition(value);
                position = value;
            }
        }
    }
}