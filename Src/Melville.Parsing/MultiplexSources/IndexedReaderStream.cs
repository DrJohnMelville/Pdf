using System.Diagnostics.CodeAnalysis;
using Melville.Parsing.AwaitConfiguration;
using Melville.Parsing.ObjectRentals;
using Melville.Parsing.Streams.Bases;

namespace Melville.Parsing.MultiplexSources;

internal interface IIndexedReader
{
    /// <summary>
    /// Read bytes into a span.
    /// </summary>
    /// <param name="position">The position to read from</param>
    /// <param name="buffer">A span to hold the read data.</param>
    /// <returns>The number of bytes read</returns>
    public int Read(long position, in Span<byte> buffer);

    /// <summary>
    /// Asynchronously read from the stream to a memory
    /// </summary>
    /// <param name="position">Position to start reading from.</param>
    /// <param name="buffer">A memory to hold the read data</param>
    /// <param name="cancellationToken">A cancellation token</param>
    /// <returns>The number of bytes read</returns>
    public ValueTask<int> ReadAsync(
        long position, Memory<byte> buffer, CancellationToken cancellationToken);

    /// <summary>
    /// The length of the represented data.
    /// </summary>
    long Length { get; }
}

internal partial class IndexedReaderStreamFactory : ObjectPoolBase<IndexedReaderStream>
{
    public static readonly IndexedReaderStreamFactory Shared = new();
    protected override IndexedReaderStream Create() => new(this);
}

internal class IndexedReaderStream(IndexedReaderStreamFactory home) : 
    DefaultBaseStream(true, false, true)
{
    private IIndexedReader? source = null;

    public IndexedReaderStream ReadFrom(IIndexedReader source, long position)
    {
        this.source = source;
        Position = position;
        return this;
    }

    public override int Read(Span<byte> buffer)
    {
        VerifyInitialized();
        var ret = source.Read(Position, buffer);
        Position += ret;
        return ret;
    }

    [MemberNotNull(nameof(source))]
    private void VerifyInitialized()
    {
        if (source is null) 
            throw new InvalidOperationException("Reader has no stream");
    }

    public override async ValueTask<int> ReadAsync(
        Memory<byte> buffer, CancellationToken cancellationToken = new CancellationToken())
    {
        VerifyInitialized();
        var ret = await source.ReadAsync(Position, buffer, cancellationToken).CA();
        Position += ret;
        return ret;
    }

    public override long Seek(long offset, SeekOrigin origin) =>
        Position = SeekTargetFromStreamStart(offset, origin);

    private long SeekTargetFromStreamStart(long offset, SeekOrigin origin) =>
        origin switch
        {
            SeekOrigin.Begin => offset,
            SeekOrigin.Current => Position + offset,
            SeekOrigin.End => (source?.Length??0) + offset,
            _ => throw new ArgumentOutOfRangeException(nameof(origin), origin, null)
        };

    public override long Length => source?.Length ?? 0;

    protected override void Dispose(bool disposing)
    {
        if (source is null) return; // prevents recursion
        source = null;
        base.Dispose(disposing);
        home.Return(this);
    }
}