using Melville.Parsing.Streams.Bases;

namespace Melville.Parsing.Streams;

/// <summary>
/// This class allows a Memory&lt;byte&gt; to be read as a stream.
/// </summary>
public partial class ReadMemoryAsStream : DefaultBaseStream
{
    private readonly Memory<byte> source;

    /// <inheritdoc />
    public override long Position { get; set; }

    /// <summary>
    /// Create a ReadMemoryAsStream
    /// </summary>
    /// <param name="source"><The source of data for the stresam/param>
    public ReadMemoryAsStream(Memory<byte> source) : base(true, false, true)
    {
        this.source = source;
    }

    /// <inheritdoc />
    public override int Read(Span<byte> buffer)
    {
        var readLength = Math.Min((int)BytesRemaining, buffer.Length);
        if (readLength < 0)
            throw new InvalidOperationException("Position greater than length.");
        if (readLength > 0)
        {
            source.Span.Slice((int)Position, (int)readLength).CopyTo(buffer);
            Position += readLength;
        }

        return readLength;
    }

    private long BytesRemaining => Length - Position;

    /// <inheritdoc />
    public override long Length => source.Length;

    /// <inheritdoc />
    public override long Seek(long offset, SeekOrigin origin) => 
        Position = origin switch
        {
            SeekOrigin.Begin => offset,
            SeekOrigin.Current => Position + offset,
            SeekOrigin.End => Length - offset,
            _ => throw new ArgumentOutOfRangeException(nameof(origin), origin, null)
        };
}