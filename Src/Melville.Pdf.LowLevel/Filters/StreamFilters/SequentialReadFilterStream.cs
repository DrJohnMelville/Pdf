using System;
using System.Buffers;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Melville.Pdf.LowLevel.Filters.StreamFilters;

public class SequentialReadFilterStream: Stream
{
    public override IAsyncResult BeginWrite(
        byte[] buffer, int offset, int count, AsyncCallback? callback, object? state) =>
        throw new NotSupportedException();
    public override void EndWrite(IAsyncResult asyncResult) =>
        throw new NotSupportedException();

    public override int Read(byte[] buffer, int offset, int count) =>
        ReadAsync(buffer, offset, count).GetAwaiter().GetResult();

    public override int ReadByte()
    {
        byte[] oneByteArray = ArrayPool<byte>.Shared.Rent(1);
        int r = Read(oneByteArray, 0, 1);
        var ret = r == 0 ? -1 : oneByteArray[0];
        ArrayPool<byte>.Shared.Return(oneByteArray);
        return ret;
    }

    public override int Read(Span<byte> buffer)
    {
        var array = ArrayPool<byte>.Shared.Rent(buffer.Length);
        var readLen = Read(array, 0, buffer.Length);
        array.AsSpan(0, readLen).CopyTo(buffer);
        ArrayPool<byte>.Shared.Return(array);
        return readLen;
    }
        
    public override Task<int> ReadAsync(
        byte[] buffer, int offset, int count, CancellationToken cancellationToken) =>
        ReadAsync(buffer.AsMemory(offset, count), cancellationToken).AsTask();
        
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