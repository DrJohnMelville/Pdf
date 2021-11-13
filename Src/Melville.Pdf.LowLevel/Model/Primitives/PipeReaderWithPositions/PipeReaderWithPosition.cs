using System;
using System.Diagnostics;
using System.IO.Pipelines;
using System.Threading;
using System.Threading.Tasks;

namespace Melville.Pdf.LowLevel.Model.Primitives.PipeReaderWithPositions;

public interface IPipeReaderWithPosition
{
    long GlobalPosition { get; }
    long Position { get; }
    ValueTask<ReadResult> ReadAsync(CancellationToken cancellationToken = default);
    void AdvanceTo(SequencePosition consumed);
    void AdvanceTo(SequencePosition consumed, SequencePosition examined);
    ValueTask AdvanceToLocalPositionAsync(long targetPosition);
    void MarkSequenceAsExamined();
}

public class PipeReaderWithPosition: IPipeReaderWithPosition
{
    private readonly PipeReader source;
    private readonly long basePosition;
    private ReadResult? currentBuffer;

    public PipeReaderWithPosition(PipeReader source, long basePosition)
    {
        this.source = source;
        this.basePosition = basePosition;
    }

    public long GlobalPosition => basePosition + Position;
    public long Position { get; private set; } = 0;
    public  async ValueTask<ReadResult> ReadAsync(CancellationToken cancellationToken = default)
    {
        if (currentBuffer.HasValue) return currentBuffer.Value;
        currentBuffer = await source.ReadAsync(cancellationToken);
        return currentBuffer.Value;
    }

    public bool TryRead(out ReadResult result)
    {
        if (currentBuffer.HasValue)
        {
            result = currentBuffer.Value;
            return true;
        }
        if (!source.TryRead(out result)) return false;
        currentBuffer = result;
        return true;
    }

    public void MarkSequenceAsExamined()
    {
        if (currentBuffer.HasValue)
            AdvanceTo(currentBuffer.Value.Buffer.Start, currentBuffer.Value.Buffer.End);
    }

    public void AdvanceTo(SequencePosition consumed)
    {
        IncrementPosition(consumed);
        source.AdvanceTo(consumed);
    }

    public void AdvanceTo(SequencePosition consumed, SequencePosition examined)
    {
        IncrementPosition(consumed);
        source.AdvanceTo(consumed, examined);
    }
    private void IncrementPosition(SequencePosition consumed)
    {
        if (!currentBuffer.HasValue)
            throw new InvalidOperationException("Must have a current sequence to advance");
        Position += currentBuffer.Value.Buffer.Slice(0, consumed).Length;
        currentBuffer = null;
    }
    
    
    public ValueTask AdvanceToLocalPositionAsync(long targetPosition) =>
        BytesToAdvanceBy(targetPosition) switch
         {
            < 0 => throw new InvalidOperationException("Cannot rewind a pipe reader"),
            0 => new ValueTask(),
            _ => TryAdvanceFast(BytesToAdvanceBy(targetPosition))
        };

    private long BytesToAdvanceBy(long targetPosition) => targetPosition - Position;

    private ValueTask TryAdvanceFast(long delta)
    {
        if (!TryRead(out var rr) || rr.Buffer.Length < delta) return SlowAdvanceToPositionAsync(delta);
        AdvanceTo(rr.Buffer.GetPosition(delta));
        return new ValueTask();

    }

    private async ValueTask SlowAdvanceToPositionAsync(long delta)
    {
        while (true)
        {
            var ret = await ReadAsync();
            if (ret.Buffer.Length > delta)
            {
                AdvanceTo(ret.Buffer.GetPosition(delta));
                return;
            }

            if (ret.IsCompleted) return;
            AdvanceTo(ret.Buffer.Start, ret.Buffer.End);
        }
    }
    
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
}