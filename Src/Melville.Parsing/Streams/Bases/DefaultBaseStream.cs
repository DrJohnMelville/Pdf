namespace Melville.Parsing.Streams.Bases;


/// <summary>
/// A stream designed to be overridden  Has default implementations of all abstract members.
/// </summary>
public abstract class DefaultBaseStream : ReadWriteStreamBase
{
    /// <summary>
    /// Create a DefaultBaseStream
    /// </summary>
    /// <param name="canRead">The stream can be read.</param>
    /// <param name="canWrite">The stream can be written.</param>
    /// <param name="canSeek">The stream can seek.</param>
    public DefaultBaseStream(bool canRead, bool canWrite, bool canSeek)
    {
        CanRead = canRead;
        CanWrite = canWrite;
        CanSeek = canSeek;
    }

    /// <inheritdoc />
    public override void Flush()
    {
    }

    /// <inheritdoc />
    public override long Seek(long offset, SeekOrigin origin)
    {
        throw new NotSupportedException();
    }

    /// <inheritdoc />
    public override void SetLength(long value)
    {
        throw new NotSupportedException();
    }

    /// <inheritdoc />
    public override bool CanRead { get; }

    /// <inheritdoc />
    public override bool CanSeek { get; }

    /// <inheritdoc />
    public override bool CanWrite { get; }

    /// <inheritdoc />
    public override long Length => 0;

    /// <inheritdoc />
    public override long Position { get; set; }
}