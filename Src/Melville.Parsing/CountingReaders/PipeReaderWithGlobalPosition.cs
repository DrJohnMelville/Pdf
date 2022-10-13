using System.IO.Pipelines;
using Melville.INPC;

namespace Melville.Parsing.CountingReaders;

public interface IByteSourceWithGlobalPosition: IByteSource
{
    long GlobalPosition { get; }
}

public partial class ByteSourceWithGlobalPosition: IByteSourceWithGlobalPosition
{
    [DelegateTo()]private readonly IByteSource source;
    private readonly long basePosition;

    public ByteSourceWithGlobalPosition(PipeReader source, long basePosition): 
        this(new ByteSource(source), basePosition){}
    
    public ByteSourceWithGlobalPosition(IByteSource source, long basePosition)
    {
        this.source = source;
        this.basePosition = basePosition - this.source.Position;
    }
    public long GlobalPosition => basePosition + Position;
}