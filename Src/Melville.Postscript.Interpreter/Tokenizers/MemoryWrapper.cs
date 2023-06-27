using System;
using System.Buffers;
using System.IO.Pipelines;
using System.Threading.Tasks;
using Melville.INPC;

namespace Melville.Postscript.Interpreter.Tokenizers;

/// <summary>
/// An ICodeSource that reads data from an IMemory source.
/// </summary>
public partial class MemoryWrapper : ICodeSource
{
    /// <summary>
    /// The buffer to read from.
    /// </summary>
    [FromConstructor] private Memory<byte> buffer;
    /// <summary>
    /// The total number of bytes consumed.
    /// </summary>
    public int BytesConsumed { get; private set; } = 0;
    private ReadOnlySequence<byte>? lastSequence;

    /// <inheritdoc />
    public ValueTask<ReadResult> ReadAsync() => ValueTask.FromResult(Read());

    /// <inheritdoc />
    public ReadResult Read()
    {
        var seq = BuildSequence();
        return new ReadResult(seq, false, true);
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
        BytesConsumed += delta;
        buffer = buffer[delta..];
        lastSequence = null;
    }

    /// <inheritdoc />
    public void AdvanceTo(SequencePosition consumed, SequencePosition examinned) =>
        AdvanceTo(consumed);
}