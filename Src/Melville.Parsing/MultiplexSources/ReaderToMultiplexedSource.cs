using Melville.IntersectionTypes;
using Melville.StreamInterfaces;

namespace Melville.Parsing.MultiplexSources;

internal sealed class ReaderToMultiplexedSource(MultiplexedReaderSource source) : IndexedMultiplexSource
{
    public override long Length => source.Length;
    private readonly SemaphoreSlim semaphore = new(1);
    protected override void CleanUp() => (source.Value as IDisposable)?.Dispose();

    void SetPosition(long position)
    {
        if (source is IStreamPosition isp && isp.Position == position) return;
        source.Seek(position);
    }
    public override async ValueTask<int> ReadAsync(long position, Memory<byte> buffer, CancellationToken cancellationToken)
    {
        await semaphore.WaitAsync();
        try
        {
            SetPosition(position);
            return await source.ReadAsync(buffer, cancellationToken);
        }
        finally
        {
            semaphore.Release();            
        }

    }

    public override int Read(long position, in Span<byte> buffer)
    {
        semaphore.Wait();
        try
        {
            SetPosition(position);
            return source.Read(buffer);
        }
        finally
        {
            semaphore.Release();
        }
    }
}

[IntersectionType]
public readonly partial struct MultiplexedReaderSource: 
    IAsyncReader, ISyncReader, ISeekableStream, IStreamLength { }
