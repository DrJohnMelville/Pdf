using System.Buffers;
using System.Diagnostics;
using System.IO.Pipelines;
using Melville.INPC;
using Melville.Parsing.AwaitConfiguration;

namespace Melville.Parsing.CountingReaders;

/// <summary>
/// Implementation of IByteSource that reads from a pipe
/// </summary>
public class ByteSource : IByteSource
{
    private PipeReader inner;
    /// <summary>
    /// Position within the source stream.
    /// </summary>
    public long Position { get; private set; }
    private ReadResult? currentBuffer;

    /// <summary>
    /// Create a ByteSource from a pipereader
    /// </summary>
    /// <param name="inner">The pipereader to get data from.</param>
    public ByteSource(PipeReader inner)
    {
        this.inner = inner;
    }

    /// <summary>
    /// Try to get a buffer if there are unexamined bytes remaining in the buffer.
    /// </summary>
    /// <param name="result">The read result that may contain read data</param>
    /// <returns>True if there are bytes available, false otherwise.</returns>
    public bool TryRead(out ReadResult result)
    {
        var succeeded = inner.TryRead(out result);
        if (succeeded)
        {
            currentBuffer = result;
        }
        return succeeded;
    }

    /// <summary>
    /// Block and read more bytes from the source.  If there are no unexamined bytes in the buffer
    /// then block and read more from the source.
    /// </summary>
    /// <param name="cancellationToken">A token that might cancel the read operation</param>
    public async ValueTask<ReadResult> ReadAsync(
        CancellationToken cancellationToken = default)
    {
        var ret = await inner.ReadAsync(cancellationToken).CA();
        currentBuffer = ret;
        return ret;
    }

    /// <summary>
    /// Mark the entire buffered sequence as examined.
    /// </summary>
    public void MarkSequenceAsExamined()
    {
        Debug.Assert(currentBuffer is not null);
        if (currentBuffer.HasValue) 
            AdvanceTo(currentBuffer.Value.Buffer.Start, currentBuffer.Value.Buffer.End);
    }


    /// <summary>
    /// Consume bytes to a given position.
    /// </summary>
    /// <param name="consumed">Position of the next byte to be read.</param>
    public void AdvanceTo(SequencePosition consumed)
    {
        IncrementPosition(consumed);
        inner.AdvanceTo(consumed);
    }

    /// <summary>
    /// Consume bytes and mark other bytes as examined.
    /// </summary>
    /// <param name="consumed">The position of the next byte to consume.</param>
    /// <param name="examined">The position of the first unexamined byte.</param>
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