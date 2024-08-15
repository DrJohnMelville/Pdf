using Melville.Parsing.CountingReaders;
using Melville.Parsing.MultiplexSources;

/// <summary>
/// This is an IMultiplexSource that represents a position inside of another IMultiplexSource,
/// but with an offset applied.
/// </summary>
/// <param name="inner">The inner IMultiplexSource that the data comes from</param>
/// <param name="offset">The offset into the original IMultiplexSource</param>
public class OffsetMultiplexSource(IMultiplexSource inner, long offset) : IMultiplexSource
{

    /// <inheritdoc />
    public void Dispose() => inner.Dispose();

    /// <inheritdoc />
    public Stream ReadFrom(long position) => inner.ReadFrom(position + offset);

    /// <inheritdoc />
    public IByteSource ReadPipeFrom(long position, long startingPosition = 0) =>
       inner.ReadPipeFrom(position + offset, startingPosition);
    
    /// <inheritdoc />
    public long Length => inner.Length - offset;
#warning and offset from an offset source should reference the original, not this source.
}