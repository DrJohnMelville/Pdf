using System.IO.Pipelines;
using Melville.INPC;

namespace Melville.Parsing.CountingReaders;

/// <summary>
/// This is a bytesource that has 2 positions, a local position within a block or
/// substream and a global position in a stream containing that block.
/// </summary>
public interface IByteSourceWithGlobalPosition: IByteSource
{
    /// <summary>
    /// The global position within the larger stream.
    /// </summary>
    long GlobalPosition { get; }
}

/// <summary>
/// An IByteSouorceWithGlobalPosition that stores a fixed offset from the beginning
/// of the source stream.
/// </summary>
public partial class ByteSourceWithGlobalPosition: IByteSourceWithGlobalPosition
{
    [DelegateTo()]private readonly IByteSource source;
    private readonly long basePosition;

    /// <summary>
    /// Creates a ByteSourceWithGlobalPosition
    /// </summary>
    /// <param name="source">The source from which additional data read.</param>
    /// <param name="basePosition">The offset between the local and global streams</param>
    public ByteSourceWithGlobalPosition(IByteSource source, long basePosition)
    {
        this.source = source;
        this.basePosition = basePosition - this.source.Position;
    }

    /// <inheritdoc />
    public long GlobalPosition => basePosition + this.source.Position;

#warning -- elimiate ByteSourceWithGlobalPosition in favor of ReusablePipereader
    public long Position =>
        throw new NotImplementedException("Try not to use Position on ByteSourceWithGlobaalPosition, because it is going away");
}