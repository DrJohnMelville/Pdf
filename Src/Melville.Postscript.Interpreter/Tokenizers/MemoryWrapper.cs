using System;
using System.Buffers;
using System.IO.Pipelines;
using System.Threading;
using System.Threading.Tasks;
using Melville.INPC;
using Melville.Parsing.CountingReaders;

namespace Melville.Postscript.Interpreter.Tokenizers;

#warning generalize this and use it in the MultiplexSources -- I can make it more efficient
/// <summary>
/// An IByteSource that reads data from an IMemory source.
/// </summary>
public partial class MemoryWrapper : IByteSource
{
    /// <summary>
    /// The buffer to read from.
    /// </summary>
    [FromConstructor] private Memory<byte> buffer;
    private ReadOnlySequence<byte>? lastSequence;

    /// <inheritdoc />
    public ValueTask<ReadResult> ReadAsync(CancellationToken token) => 
        ValueTask.FromResult(((IByteSource)this).Read());

    /// <inheritdoc />
    public bool TryRead(out ReadResult result)
    {
        var seq = BuildSequence();
        result = new ReadResult(seq, false, true);
        return true;
    }

    private ReadOnlySequence<byte> BuildSequence()
    {
        var builder = new ReadOnlySequenceBuilder<byte>();
        builder.Append(buffer);
        lastSequence = builder.GetSequence();
        return lastSequence.Value;
    }

    /// <inheritdoc />
    public void AdvanceTo(SequencePosition consumed)
    {
        if (lastSequence is not {} seq)
            throw new InvalidOperationException("Advance without read");
        var delta = (int)seq.Slice(seq.Start, consumed).Length;
        Position += delta;
        buffer = buffer[delta..];
        lastSequence = null;
    }

    /// <inheritdoc />
    public void AdvanceTo(SequencePosition consumed, SequencePosition examined) =>
        AdvanceTo(consumed);

    /// <inheritdoc />
    public long GlobalPosition => Position;

    /// <inheritdoc />
    public void MarkSequenceAsExamined()
    {
        // do nothing
    }

    /// <inheritdoc />
    public long Position { get; private set; }

    /// <inheritdoc />
    public void Dispose()
    {
    }
}