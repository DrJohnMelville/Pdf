using System.Buffers;
using System.IO.Pipelines;
using Melville.INPC;

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
        var ret = await inner.ReadAsync(cancellationToken);
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
}