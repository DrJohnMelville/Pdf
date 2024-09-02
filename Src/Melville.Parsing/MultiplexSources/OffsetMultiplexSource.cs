using Melville.INPC;
using Melville.Parsing.CountingReaders;

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
    [FromConstructor]private readonly IMultiplexSource inner1;
    /// <summary>
    /// The offset into the original IMultiplexSource
    /// </summary>
    [FromConstructor]private readonly long offset1;

    /// <inheritdoc />
    public void Dispose() { }

    /// <inheritdoc />
    public Stream ReadFrom(long position) => inner1.ReadFrom(position + offset1);

    /// <inheritdoc />
    public IByteSource ReadPipeFrom(long position, long startingPosition = 0) =>
       inner1.ReadPipeFrom(position + offset1, startingPosition);
    
    /// <inheritdoc />
      public long Length => inner1.Length - offset1;

    /// <inheritdoc />
    public IMultiplexSource OffsetFrom(uint newOffset)
    {
        return inner1.OffsetFrom((uint)offset1+newOffset);
    }
}