namespace Melville.Parsing.Streams.Bases;

public abstract class DefaultBaseStream : ReadWriteStreamBase
{
    public DefaultBaseStream(bool canRead, bool canWrite, bool canSeek)
    {
        CanRead = canRead;
        CanWrite = canWrite;
        CanSeek = canSeek;
    }

    public override void Flush()
    {
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
        throw new NotSupportedException();
    }

    public override void SetLength(long value)
    {
        throw new NotSupportedException();
    }

    public override bool CanRead { get; }

    public override bool CanSeek { get; }

    public override bool CanWrite { get; }

    public override long Length => 0;

    public override long Position
    {
        get => 0;
        set => Seek(value, SeekOrigin.Begin);
    }
}