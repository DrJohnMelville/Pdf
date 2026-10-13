using Melville.INPC;
using Melville.Parsing.CountingReaders;
using Melville.StreamInterfaces;

namespace Melville.Parsing.MultiplexSources;

/// <summary>
/// This is an IMultiplexSource that represents a position inside of another IMultiplexSource,
/// but with an offset applied.
/// </summary>
public partial class OffsetMultiplexSource : IMultiplexSource
{
    /// <summary>
    /// The inner IMultiplexSource that the data comes from
    /// </summary>
    [FromConstructor]private readonly IMultiplexSource inner;
    /// <summary>
    /// The offset into the original IMultiplexSource
    /// </summary>
    [FromConstructor]private readonly long offset;

    /// <inheritdoc />
    public void Dispose() { }

    /// <inheritdoc />
    public Stream ReadFrom(long position) => inner.ReadFrom(position + offset);

    /// <inheritdoc/>
    public IAsyncReader ReaderFrom(long position) => inner.ReaderFrom(position + offset);

    /// <inheritdoc />
    public IByteSource ReadPipeFrom(long position, long startingPosition = 0) =>
       inner.ReadPipeFrom(position + offset, startingPosition);
    
    /// <inheritdoc />
      public long Length => inner.Length - offset;

    /// <inheritdoc />
    public IMultiplexSource OffsetFrom(uint newOffset)
    {
        return inner.OffsetFrom((uint)offset+newOffset);
    }
}