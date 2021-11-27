using System.IO.Pipelines;

namespace Melville.Parsing.CountingReaders;

public interface IPipeReaderWithPosition
{
    long GlobalPosition { get; }
    ValueTask<ReadResult> ReadAsync(CancellationToken cancellationToken = default);
    void AdvanceTo(SequencePosition consumed);
    void AdvanceTo(SequencePosition consumed, SequencePosition examined);
    ValueTask AdvanceToLocalPositionAsync(long targetPosition);
    void MarkSequenceAsExamined();
}

public class PipeReaderWithPosition: IPipeReaderWithPosition
{
    public CountingPipeReader Source { get; }
    private readonly long basePosition;

    public PipeReaderWithPosition(PipeReader source, long basePosition)
    {
        Source = source as CountingPipeReader ?? new CountingPipeReader(source);
        this.basePosition = basePosition - Source.Position;
    }

    public long GlobalPosition => basePosition + Position;
    public long Position => Source.Position;

    public ValueTask<ReadResult> ReadAsync(CancellationToken cancellationToken = default) =>
        Source.ReadAsync(cancellationToken);

    public bool TryRead(out ReadResult result) => Source.TryRead(out result);

    public void MarkSequenceAsExamined() => Source.MarkSequenceAsExamined();

    public void AdvanceTo(SequencePosition consumed) => Source.AdvanceTo(consumed);

    public void AdvanceTo(SequencePosition consumed, SequencePosition examined) =>
        Source.AdvanceTo(consumed, examined);


    public ValueTask AdvanceToLocalPositionAsync(long targetPosition) =>
        Source.AdvanceToLocalPositionAsync(targetPosition);
}

public static class PipeReaderWithPositionOperations
{
    /// <summary>
    /// This method enables a very specific pattern that is common with parsing from the PipeReader.
    ///
    /// the pattern is do{}while(source.ShouldContinue(Method(await source.ReadAsync)));
    ///
    /// Method returns a pair (bool, SequencePosition).  Method can use out parameters for "real"
    /// return values.
    ///
    /// This pattern repeately reads the stream until method successfully parses, then it advances
    /// the reader to the given sequence position.
    /// </summary>
    public static bool ShouldContinue(
        this IPipeReaderWithPosition pipe, (bool Success, SequencePosition Position) result)
    {
        if (result.Success)
        {
            pipe.AdvanceTo(result.Position);
            return false;
        }

        pipe.MarkSequenceAsExamined();
        return true;
    }    
    
    /// <summary>
    /// This method enables a very specific pattern that is common with parsing from the PipeReader.
    ///
    /// the pattern is do{}while(source.ShouldContinue(Method(await source.ReadAsync)));
    ///
    /// Method returns a pair (bool, SequencePosition).  Method can use out parameters for "real"
    /// return values.
    ///
    /// This pattern repeately reads the stream until method successfully parses, then it advances
    /// the reader to the given sequence position.
    /// </summary>
    public static bool ShouldContinue(
        this CountingPipeReader pipe, (bool Success, SequencePosition Position) result)
    {
        if (result.Success)
        {
            pipe.AdvanceTo(result.Position);
            return false;
        }

        pipe.MarkSequenceAsExamined();
        return true;
    }
}