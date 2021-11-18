using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Filters.FlateFilters;

namespace Melville.Wpf.IntegrationTesting;

public class WriteToAdlerStream: Stream
{
    public Adler32Computer Computer { get; } = new();
    public override IAsyncResult BeginWrite(
        byte[] buffer, int offset, int count, AsyncCallback? callback, object? state) =>
        throw new NotSupportedException();
    public override void EndWrite(IAsyncResult asyncResult) =>
        throw new NotSupportedException();

    // for reasons I cannot understand PngBitmapEncoder tries to read the output stream, even
    // when CanRead is false.  The solution is just to simulate an empty stream on the read
    // operations.
    public override int Read(byte[] buffer, int offset, int count) => 0;
    public override int ReadByte() => 0;
    public override int Read(Span<byte> buffer) => 0;
    public override Task<int> ReadAsync(
        byte[] buffer, int offset, int count, CancellationToken cancellationToken) =>
        Task.FromResult(0);

    public override void Write(byte[] buffer, int offset, int count) =>
        Write(buffer.AsSpan(offset, count));

    public override void Write(ReadOnlySpan<byte> buffer)
    {
        Position += buffer.Length;
        Computer.AddData(buffer);
    }

    public override Task WriteAsync(
        byte[] buffer, int offset, int count, CancellationToken cancellationToken) =>
        ReadAsync(buffer.AsMemory(offset, count), cancellationToken).AsTask();

    public override ValueTask WriteAsync(
        ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = new CancellationToken())
    {
        Write(buffer.Span);
        return ValueTask.CompletedTask;
    }

    public override void WriteByte(byte value)
    {
        Span<byte> val = stackalloc byte[] { value };
        Write(val);
    }

    public override void Flush()
    {
    }

    public override Task FlushAsync(CancellationToken cancellationToken) =>
        Task.CompletedTask;

    public override long Seek(long offset, SeekOrigin origin)=>
        throw new NotSupportedException();

    public override void SetLength(long value)=>
        throw new NotSupportedException();

    public override bool CanRead => false;
    public override bool CanSeek => false;
    public override bool CanTimeout => false;
    public override bool CanWrite => true;
    public override long Length => Position;
    public override long Position { get; set; }
}