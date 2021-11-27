using System.IO.Pipelines;

namespace Melville.Parsing.CountingReaders;

public interface IPipeReaderWithPosition
{
    long GlobalPosition { get; }
    CountingPipeReader Source { get; }
}

public class PipeReaderWithPosition: IPipeReaderWithPosition
{
    public CountingPipeReader Source { get; }
    private readonly long basePosition;

    public PipeReaderWithPosition(PipeReader source, long basePosition)
    {
        Source = source.AsCountingPipeReader();
        this.basePosition = basePosition - Source.Position;
    }
    public long GlobalPosition => basePosition + Position;
    public long Position => Source.Position;
}