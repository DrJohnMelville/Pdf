using Melville.INPC;

namespace Melville.Parsing.Streams;

/// <summary>
/// A stream wrapper than only reasd one character at a time.  This is used to force some debugging situations.
/// </summary>
public partial class OneCharAtATimeStream : Stream
{
    [DelegateTo(Exclude = "LifetimeService")] private Stream source;

    /// <summary>
    /// Create an OneCharAtAtimeStream from an array
    /// </summary>
    /// <param name="source">The source data</param>
    public OneCharAtATimeStream(byte[] source): this(new MemoryStream(source)){ { }
    }

    /// <summary>
    /// Create an OneCharAtAtimeStream from an array
    /// </summary>
    /// <param name="source">The source data</param>
    public OneCharAtATimeStream(Stream source)
    {
        this.source = source;
    }

    /// <inheritdoc />
    public override IAsyncResult BeginRead(
        byte[] buffer, int offset, int count, AsyncCallback? callback, object? state) =>
        throw new NotSupportedException();

    /// <inheritdoc />
    public override int Read(byte[] buffer, int offset, int count) => 
        source.Read(buffer, offset, 1);

    /// <inheritdoc />
    public override int Read(Span<byte> buffer) => buffer.Length > 0?source.Read(buffer.Slice(0, 1)):0;

    /// <inheritdoc />
    public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken) => 
        source.ReadAsync(buffer, offset, 1, cancellationToken);

    /// <inheritdoc />
    public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default) => 
        source.ReadAsync(buffer.Slice(0, 1), cancellationToken);
}