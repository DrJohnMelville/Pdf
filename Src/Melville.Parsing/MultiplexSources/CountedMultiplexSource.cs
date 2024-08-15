using Melville.Parsing.CountingReaders;

namespace Melville.Parsing.MultiplexSources;

internal abstract class CountedMultiplexSource : IMultiplexSource
{
    public void Dispose()
    {
        CleanUp();
    }

    public Stream ReadFrom(long position)
    {
        return ReadFromOverride(position);
    }

    public IByteSource ReadPipeFrom(long position, long startingPosition = 0)
    {
        return ReadFromPipeOverride(position, startingPosition);
    }

    public abstract long Length { get; }

    public abstract Stream ReadFromOverride(long position);
    public virtual IByteSource ReadFromPipeOverride(long position, long startingPosition) =>
        MultiplexSourceFactory.SingleReaderForStream(this.ReadFromOverride(position))
            .WithCurrentPosition(startingPosition);
    //NB -- we call ReadFromOverride rather than ReadFrom, because we only hold one reference to this, which is
    // the stream reader, but ReadFromPipe already incremented our reference count, which the singlestreamreader will
    // not clear.  Thus the StreamReader, created without incrementing the count, will decrement the count created for
    // the IByteSource that does not.

    protected abstract void CleanUp();
}