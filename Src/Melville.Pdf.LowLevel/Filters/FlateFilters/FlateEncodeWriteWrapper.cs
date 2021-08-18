using System;
using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;

namespace Melville.Pdf.LowLevel.Filters.FlateFilters
{
    public class FlateEncodeWriteWrapper : Stream
    {
        private readonly Stream rawDestination;
        private readonly DeflateStream encodingDestination;
        private readonly Adler32Computer adler = new();

        public FlateEncodeWriteWrapper(Stream rawDestination)
        {
            this.rawDestination = rawDestination;
            WritePrefix(rawDestination);
            encodingDestination = new DeflateStream(rawDestination, CompressionLevel.Optimal);
        }

        private static byte[] prefixBytes = { 0x78, 0xDA };
        private void WritePrefix(Stream s) => s.Write(prefixBytes, 0, 2);

        public override void Flush() => encodingDestination.Flush();

        protected override void Dispose(bool disposing)
        {
            encodingDestination.Flush();
            WriteAdler32Trailer();
            rawDestination.Dispose();
        }

        private void WriteAdler32Trailer()
        {
            Span<byte> trailer = stackalloc byte[4];
            adler.CopyHashToBigEndianSpan(trailer);
            rawDestination.Write(trailer);
        }

        public override int Read(byte[] buffer, int offset, int count) =>
            throw new NotSupportedException();

        public override long Seek(long offset, SeekOrigin origin)=>
            throw new NotSupportedException();

        public override void SetLength(long value)=>
            throw new NotSupportedException();

        public override void Write(byte[] buffer, int offset, int count)
        {
            adler.AddData(buffer.AsSpan(offset, count));
            encodingDestination.Write(buffer, offset, count);
        }

        public override ValueTask WriteAsync(
            ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = new CancellationToken())
        {
            adler.AddData(buffer.Span);
            return encodingDestination.WriteAsync(buffer, cancellationToken);
        }

        public override void Write(ReadOnlySpan<byte> buffer)
        {
            adler.AddData(buffer);
            encodingDestination.Write(buffer);
        }

        public override Task WriteAsync(
            byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            adler.AddData(buffer.AsSpan(offset, count));
            return encodingDestination.WriteAsync(buffer, offset, count, cancellationToken);
        }

        public override bool CanRead => false;

        public override bool CanSeek => false;

        public override bool CanWrite => true;

        public override long Length => encodingDestination.Length;

        public override long Position
        {
            get => encodingDestination.Position;
            set => throw new NotSupportedException();
        }
    }
}