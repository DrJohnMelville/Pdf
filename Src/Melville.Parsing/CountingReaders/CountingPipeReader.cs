using System.Buffers;
using System.Diagnostics;
using System.IO.Pipelines;
using Melville.INPC;
using Melville.Parsing.AwaitConfiguration;

namespace Melville.Parsing.CountingReaders;

public class ByteSource : IByteSource
{
    private PipeReader inner;
    public long Position { get; private set; }
    private ReadResult? currentBuffer;

    public ByteSource(PipeReader inner)
    {
        this.inner = inner;
    }

    public bool TryRead(out ReadResult result)
    {
        var succeeded = inner.TryRead(out result);
        if (succeeded)
        {
            currentBuffer = result;
        }
        return succeeded;
    }
    public async ValueTask<ReadResult> ReadAsync(
        CancellationToken cancellationToken = default)
    {
        var ret = await inner.ReadAsync(cancellationToken).CA();
        currentBuffer = ret;
        return ret;
    }

    public void MarkSequenceAsExamined()
    {
        Debug.Assert(currentBuffer is not null);
        if (currentBuffer.HasValue) 
            AdvanceTo(currentBuffer.Value.Buffer.Start, currentBuffer.Value.Buffer.End);
    }


    public void AdvanceTo(SequencePosition consumed)
    {
        IncrementPosition(consumed);
        inner.AdvanceTo(consumed);
    }

    public void AdvanceTo(SequencePosition consumed, SequencePosition examined)
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