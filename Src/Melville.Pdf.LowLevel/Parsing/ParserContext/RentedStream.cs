using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using Melville.Parsing.Streams.Bases;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.StreamInterfaces;

namespace Melville.Pdf.LowLevel.Parsing.ParserContext;

internal partial class ParsingFileOwner
{

    public partial class RentedReader (IAsyncReader reader, long length) : IPdfStreamReader, IStreamLength, IStreamPosition
    {
        public long Length { get; } = length;

        public long Position { get; private set; }

        public void Dispose() => (reader as IDisposable)?.Dispose();

        public async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellation = default)
        {
            buffer = buffer.TakeMax(ClampToInt(Length - Position));
            if (buffer.IsEmpty) return 0;
            var ret = await reader.ReadAsync(buffer);
            Position += ret;
            return ret;
        }

        public int ClampToInt(long value) => (int)long.Clamp(value, 0, int.MaxValue);
    }

    public partial class RentedStream: DefaultBaseStream
    {
        private readonly Stream baseStream;
        private readonly long startPosition;
        private readonly long nextPosition;

        public RentedStream(Stream baseStream, long length):base(true, false, true)
        {
            this.baseStream = baseStream;
            this.startPosition = baseStream.Position;
            this.nextPosition = startPosition + length;
        }

        public override int Read(Span<byte> buffer)
        {
            var avail = nextPosition - baseStream.Position;
            if (avail <= 0) return 0;
            buffer = buffer.TakeMax((int)avail);
            return baseStream.Read(buffer);
        }

        public override async ValueTask<int> ReadAsync(Memory<byte> buffer, System.Threading.CancellationToken cancellationToken = new System.Threading.CancellationToken())
        {
            var avail = nextPosition - baseStream.Position;
            if (avail <= 0) return 0;
            if (avail < buffer.Length) buffer = buffer[..(int)avail];
            return await baseStream.ReadAsync(buffer, cancellationToken).CA();
        }

        public override long Seek(long offset, SeekOrigin origin) =>
            origin switch
            {
                SeekOrigin.Begin => baseStream.Seek(offset + startPosition, SeekOrigin.Begin),
                SeekOrigin.Current => baseStream.Seek(offset, SeekOrigin.Current),
                SeekOrigin.End => baseStream.Seek(nextPosition - offset, SeekOrigin.Begin),
                _ => throw new ArgumentOutOfRangeException(nameof(origin), origin, null)
            };

        public override void SetLength(long value) =>
            throw new NotSupportedException("Cannot change the length of a rented stream.");

        public override long Length => nextPosition - startPosition;
        public override long Position
        {
            get => baseStream.Position - startPosition;
            set => baseStream.Position = value + startPosition;
        }

        protected override void Dispose(bool disposing)
        {
            baseStream.Dispose();
            base.Dispose(disposing);
        }

        public override async ValueTask DisposeAsync()
        {
            await baseStream.DisposeAsync().CA();
            await base.DisposeAsync().CA();
        }
    }
}