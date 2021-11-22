using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.Wrappers.Functions.PostScriptInterpreter;

namespace Melville.Pdf.LowLevel.Parsing.ParserContext.MultiplexedStreams;

public sealed class MultiplexedStream: IDisposable
{
    private Stream source;
    public long Length => source.Length;
    private SemaphoreSlim mutex = new SemaphoreSlim(1);

    public MultiplexedStream(Stream source)
    {
        this.source = source;
        VerifyLegalStream();
    }

    public void Dispose() => mutex.Dispose();

    private void VerifyLegalStream()
    {
        if (!source.CanRead) throw new ArgumentException("Stream must be readable.");
        if (!source.CanSeek) throw new ArgumentException("Stream must be seekable.");
    }

    public int Read(long position, in Span<byte> buffer)
    {
        mutex.Wait();
        try
        {
            EnsureProperReadPosition(position);
            return source.Read(buffer);
        }
        finally
        {
            mutex.Release();
        }
    }

    public async ValueTask<int> ReadAsync(
        long position, Memory<byte> buffer, CancellationToken cancellationToken)
    {
        EnsureProperReadPosition(position);
        await mutex.WaitAsync();
        try
        {
            return await source.ReadAsync(buffer, cancellationToken);
        }
        finally
        {
            mutex.Release();
        }
    }

    private void EnsureProperReadPosition(long position)
    {
        if (source.Position != position) source.Seek(position, SeekOrigin.Begin);
    }

    public Stream ReadFrom(long position) => new MultiplexedReader(this, position);
}

public sealed class MultiplexedReader : Stream
{
    private readonly MultiplexedStream source;

    public MultiplexedReader(MultiplexedStream source, long position)
    {
        this.source = source;
        Position = position;
    }

    public override void Flush()
    {
    }

    public override int Read(byte[] buffer, int offset, int count) =>
        Read(buffer.AsSpan(offset, count));
    

    public override int Read(Span<byte> buffer)
    {
        var ret = source.Read(Position, buffer);
        Position += ret;
        return ret;
    }

    public override Task<int> ReadAsync(
        byte[] buffer, int offset, int count, CancellationToken cancellationToken) =>
        ReadAsync(buffer.AsMemory(offset, count), cancellationToken).AsTask();
    

    public override async ValueTask<int> ReadAsync(
        Memory<byte> buffer, CancellationToken cancellationToken = new CancellationToken())
    {
        var ret = await source.ReadAsync(Position, buffer, cancellationToken);
        Position += ret;
        return ret;
    }
    
    public override long Seek(long offset, SeekOrigin origin) =>
        Position = SeekTargetFromStreamStart(offset, origin);

    private long SeekTargetFromStreamStart(long offset, SeekOrigin origin) =>
        origin switch {
            SeekOrigin.Begin => offset,
            SeekOrigin.Current => Position + offset,
            SeekOrigin.End => source.Length + offset,
            _ => throw new ArgumentOutOfRangeException(nameof(origin), origin, null)
        };

    public override void SetLength(long value) => throw new NotSupportedException();

    public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();

    public override bool CanRead => true;
    public override bool CanSeek => true;
    public override bool CanWrite => false;
    public override long Length => source.Length;

    public override long Position { get; set; }
}