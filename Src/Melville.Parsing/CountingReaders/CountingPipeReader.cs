using System.Buffers;
using System.IO.Pipelines;
using Melville.INPC;
using Melville.Parsing.AwaitConfiguration;

namespace Melville.Parsing.CountingReaders;

public partial class CountingPipeReader : PipeReader
{
    [DelegateTo()] private PipeReader inner;
    public long Position { get; private set; }
    private ReadResult? currentBuffer;

    public CountingPipeReader(PipeReader inner)
    {
        this.inner = inner;
    }

    public override bool TryRead(out ReadResult result)
    {
        var succeeded = inner.TryRead(out result);
        if (succeeded)
        {
            currentBuffer = result;
        }
        return succeeded;
    }
    public override async ValueTask<ReadResult> ReadAsync(
        CancellationToken cancellationToken = default)
    {
        var ret = await inner.ReadAsync(cancellationToken).CA();
        currentBuffer = ret;
        return ret;
    }

    public void MarkSequenceAsExamined()
    {
        if (currentBuffer.HasValue) 
            AdvanceTo(currentBuffer.Value.Buffer.Start, currentBuffer.Value.Buffer.End);
    }

    public override void AdvanceTo(SequencePosition consumed)
    {
        IncrementPosition(consumed);
        inner.AdvanceTo(consumed);
    }

    public override void AdvanceTo(SequencePosition consumed, SequencePosition examined)
    {
        IncrementPosition(consumed);
        inner.AdvanceTo(consumed, examined);
    }

    private void IncrementPosition(SequencePosition consumed)
    {
        if (!currentBuffer.HasValue) throw new InvalidOperationException("No buffer to advance within");
        Position += currentBuffer.Value.Buffer.Slice(0, consumed).Length;
    }

    #region AdvanceToLocalPosition
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
            var ret = await ReadAsync().CA();
            if (ret.Buffer.Length > delta)
            {
                AdvanceTo(ret.Buffer.GetPosition(delta));
                return;
            }

            if (ret.IsCompleted) return;
            AdvanceTo(ret.Buffer.Start, ret.Buffer.End);
        }
    }
    #endregion
}

public static class CountingPipeReaderOperations
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

    public static CountingPipeReader AsCountingPipeReader(this PipeReader pr) =>
        pr as CountingPipeReader ?? new CountingPipeReader(pr);
}