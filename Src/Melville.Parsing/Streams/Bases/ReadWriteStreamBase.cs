using System.Buffers;

namespace Melville.Parsing.Streams.Bases;

//Everything defers to Read/Write(Span) and ReadAsync/WriteAsync(Memory) which are implemented in terms of each other.
// overriding ont of these will make all 5 methods work, overriding bothe the sync and async version will give
// better perf.
public abstract class ReadWriteStreamBase: Stream
{
    public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback? callback, object? state) => 
        ReadAsync(buffer, offset, count).AsApm(callback, state);

    public override int EndRead(IAsyncResult asyncResult) => ((Task<int>)asyncResult).Result;
    
    public override int Read(byte[] buffer, int offset, int count) => 
        ReadAsync(buffer.AsMemory(offset, count)).GetAwaiter().GetResult();

    public override int Read(Span<byte> buffer)
    {
        var rented = ArrayPool<byte>.Shared.Rent(buffer.Length);
        var ret = ReadAsync(rented[..buffer.Length]).GetAwaiter().GetResult();
        rented.AsSpan(0, ret).CopyTo(buffer);
        ArrayPool<byte>.Shared.Return(rented);
        return ret;
    }

    public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
    {
        return ReadAsync(buffer.AsMemory(offset, count), cancellationToken).AsTask();
    }

    public override ValueTask<int> ReadAsync(
        Memory<byte> buffer, CancellationToken cancellationToken = new CancellationToken()) =>
        new(Read(buffer.Span));

    public override int ReadByte()
    {
        Span<byte> b = stackalloc byte[1];
        Read(b);
        return b[0];
    }

    public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback? callback, object? state) => 
        WriteAsync(buffer, offset, count).AsApm(callback, state);

    public override void EndWrite(IAsyncResult asyncResult) => ((Task)asyncResult).GetAwaiter().GetResult();

    public override void Write(byte[] buffer, int offset, int count) => 
        WriteAsync(buffer.AsMemory(offset, count)).GetAwaiter().GetResult();

    public override void Write(ReadOnlySpan<byte> buffer)
    {
        var rented = ArrayPool<byte>.Shared.Rent(buffer.Length);
        buffer.CopyTo(rented);
        WriteAsync(rented[..buffer.Length]).GetAwaiter().GetResult();
        ArrayPool<byte>.Shared.Return(rented);
    }

    public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken) =>
        WriteAsync(buffer.AsMemory(offset, count), cancellationToken).AsTask();

    public override ValueTask WriteAsync(
        ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = new CancellationToken())
    {
        Write(buffer.Span);
        return ValueTask.CompletedTask;
    }

    public override void WriteByte(byte value)
    {
        Span<byte> buff = stackalloc byte[] { value };
        Write(buff);
    }
}