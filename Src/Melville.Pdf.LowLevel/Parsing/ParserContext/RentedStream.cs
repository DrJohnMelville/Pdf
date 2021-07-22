using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Melville.INPC;

namespace Melville.Pdf.LowLevel.Parsing.ParserContext
{
    public partial class ParsingFileOwner
    {
        public partial class RentedStream : Stream
        {
            [DelegateTo()]
            private readonly Stream baseStream;
            private readonly ParsingFileOwner owner;
            private readonly long basePosition;
            private readonly long length;

            public RentedStream(Stream baseStream, long length, ParsingFileOwner owner)
            {
                this.baseStream = baseStream;
                this.length = length;
                this.owner = owner;
                basePosition = baseStream.Position;
            }

            private int RemainingBytes => (int)(basePosition + length - baseStream.Position);
            private int MaxBytes(int count) => Math.Min(count, RemainingBytes);

            #region Read Methods

            public override IAsyncResult BeginRead(
                byte[] buffer, int offset, int count, AsyncCallback? callback, object? state) => 
                baseStream.BeginRead(buffer, offset, MaxBytes(count), callback, state);
            public override int Read(byte[] buffer, int offset, int count) => 
                baseStream.Read(buffer, offset, MaxBytes(count));
            public override int Read(Span<byte> buffer) => 
                baseStream.Read(buffer[..RemainingBytes]);
            public override Task<int> ReadAsync(
                byte[] buffer, int offset, int count, CancellationToken cancellationToken) => 
                baseStream.ReadAsync(buffer, offset, MaxBytes(count), cancellationToken);
            public override ValueTask<int> ReadAsync(
                Memory<byte> buffer, CancellationToken cancellationToken) => 
                baseStream.ReadAsync(buffer[..RemainingBytes], cancellationToken);
            public override int ReadByte()
            {
                if (RemainingBytes < 1)
                    throw new IOException("Read of end of a rented stream"); 
                return baseStream.ReadByte();
            }

            #endregion

            #region Close and dispose

            public override ValueTask DisposeAsync()
            { 
                Close();
                return ValueTask.CompletedTask;
            }
            protected override void Dispose(bool disposing) => Close();
            public override void Close() => owner.ReturnReader(this);
            

            #endregion

            #region Seek, Position and Length

            public override long Seek(long offset, SeekOrigin origin) =>
                origin switch
                {
                    SeekOrigin.Begin => baseStream.Seek(offset + basePosition, SeekOrigin.Begin),
                    SeekOrigin.Current => baseStream.Seek(offset, SeekOrigin.Current),
                    SeekOrigin.End => baseStream.Seek(basePosition + length - offset, SeekOrigin.Begin),
                    _ => throw new ArgumentOutOfRangeException(nameof(origin), origin, null)
                };

            public override void SetLength(long value) => 
                throw new NotSupportedException("Cannot change the length of a rented stream.");

            public override long Length => length;
            public override long Position
            {
                get => baseStream.Position - basePosition ;
                set => baseStream.Position = value + basePosition;
            }

            #endregion

            #region Writer methods

            private int CheckLength(int len)
            {
                if (len > RemainingBytes)
                    throw new IOException("Write off the end of a rented stream");
                return len;
            }

            private ReadOnlyMemory<byte> CheckLength(ReadOnlyMemory<byte> buffer)
            {
                CheckLength(buffer.Length);
                return buffer;
            }

            private ReadOnlySpan<Byte> CheckLength(ReadOnlySpan<byte> buffer)
            {
                CheckLength(buffer.Length);
                return buffer;
            }

            public override IAsyncResult BeginWrite(
                byte[] buffer, int offset, int count, AsyncCallback? callback, object? state) => 
                baseStream.BeginWrite(buffer, offset, CheckLength(count), callback, state);

            public override void Write(byte[] buffer, int offset, int count) => 
                baseStream.Write(buffer, offset, CheckLength(count));

            public override void Write(ReadOnlySpan<byte> buffer) => 
                baseStream.Write(CheckLength(buffer));

            public override Task WriteAsync(
                byte[] buffer, int offset, int count, CancellationToken cancellationToken) => 
                baseStream.WriteAsync(buffer, offset, CheckLength(count), cancellationToken);

            public override ValueTask WriteAsync(
                ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default) => 
                baseStream.WriteAsync(CheckLength(buffer), cancellationToken);

            public override void WriteByte(byte value)
            {
                CheckLength(1);
                baseStream.WriteByte(value);
            }

            #endregion
        }
    }
}