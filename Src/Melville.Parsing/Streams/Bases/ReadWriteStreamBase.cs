using System.Buffers;
using Melville.Parsing.AwaitConfiguration;

namespace Melville.Parsing.Streams.Bases;

//Everything defers to Read/Write(Span) and ReadAsync/WriteAsync(Memory) which are implemented in terms of each other.
// overriding ont of these will make all 5 methods work, overriding bothe the sync and async version will give
// better perf.

/// <summary>
/// This is a base class that simplifies the multiple methods streams can be consumed.
/// </summary>
public abstract class ReadWriteStreamBase: Stream
{
    /// <inheritdoc />
    public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback? callback, object? state) => 
        ReadAsync(buffer, offset, count).AsApm(callback, state);

    /// <inheritdoc />
    public override int EndRead(IAsyncResult asyncResult) => ((Task<int>)asyncResult).Result;

    /// <inheritdoc />
    public override int Read(byte[] buffer, int offset, int count) =>
        RunSynchronous.Do(() => ReadAsync(buffer.AsMemory(offset, count)));

    /// <inheritdoc />
    public override int Read(Span<byte> buffer)
    {
        var rented = ArrayPool<byte>.Shared.Rent(buffer.Length);
        var capturedLength = buffer.Length;
        var ret = RunSynchronous.Do(() => ReadAsync(rented.AsMemory(0, capturedLength)));
        rented.AsSpan(0, ret).CopyTo(buffer);
        ArrayPool<byte>.Shared.Return(rented);
        return ret;
    }

    /// <inheritdoc />
    public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken) => 
        ReadAsync(buffer.AsMemory(offset, count), cancellationToken).AsTask();

    /// <inheritdoc />
    public override ValueTask<int> ReadAsync(
        Memory<byte> buffer, CancellationToken cancellationToken = new CancellationToken()) =>
        new(Read(buffer.Span));

    /// <inheritdoc />
    public override int ReadByte()
    {
        Span<byte> b = stackalloc byte[1];
        var ret = Read(b);
        return ret == 0? -1:b[0];
    }

    /// <inheritdoc />
    public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback? callback, object? state) => 
        WriteAsync(buffer, offset, count).AsApm(callback, state);

    /// <inheritdoc />
    public override void EndWrite(IAsyncResult asyncResult) => ((Task)asyncResult).GetAwaiter().GetResult();

    /// <inheritdoc />
    public override void Write(byte[] buffer, int offset, int count) => 
        RunSynchronous.Do(()=>WriteAsync(buffer.AsMemory(offset, count)));

    /// <inheritdoc />
    public override void Write(ReadOnlySpan<byte> buffer)
    {
        var rented = ArrayPool<byte>.Shared.Rent(buffer.Length);
        buffer.CopyTo(rented);
        var captureLength = buffer.Length;
        RunSynchronous.Do(() => WriteAsync(rented.AsMemory(0, captureLength)));
        ArrayPool<byte>.Shared.Return(rented);
    }

    /// <inheritdoc />
    public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken) =>
        WriteAsync(buffer.AsMemory(offset, count), cancellationToken).AsTask();

    /// <inheritdoc />
    public override ValueTask WriteAsync(
        ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = new CancellationToken())
    {
        Write(buffer.Span);
        return ValueTask.CompletedTask;
    }

    /// <inheritdoc />
    public override void WriteByte(byte value)
    {
        Span<byte> buff = stackalloc byte[] { value };
        Write(buff);
    }
}