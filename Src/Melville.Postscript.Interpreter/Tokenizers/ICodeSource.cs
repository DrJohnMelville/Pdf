using Melville.INPC;
using System;
using System.Buffers;
using System.IO.Pipelines;
using System.Threading.Tasks;

namespace Melville.Postscript.Interpreter.Tokenizers;

public interface ICodeSource
{
    ValueTask<ReadResult> ReadAsync();
    internal ReadResult Read();
    void AdvanceTo(SequencePosition consumed);
    void AdvanceTo(SequencePosition consumed, SequencePosition examinned);
}

public partial class PipeWrapper : ICodeSource
{
    [FromConstructor] private readonly PipeReader reader;

    public ValueTask<ReadResult> ReadAsync()
    {
        return reader.ReadAsync();
    }

    public ReadResult Read()
    {
        throw new NotSupportedException(
            "A Pipewrapper must be read using the async method");
    }

    public void AdvanceTo(SequencePosition consumed) => reader.AdvanceTo(consumed);

    public void AdvanceTo(SequencePosition consumed, SequencePosition examinned) =>
        reader.AdvanceTo(consumed, examinned);
}

public partial class MemoryWrapper : ICodeSource
{
    [FromConstructor] private Memory<byte> buffer;
    public int BytesConsumed { get; private set; } = 0;
    private ReadOnlySequence<byte>? lastSequence;

    public ValueTask<ReadResult> ReadAsync() => ValueTask.FromResult(Read());

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

    public void AdvanceTo(SequencePosition consumed)
    {
        if (lastSequence is not {} seq)
            throw new InvalidOperationException("Advance without read");
        var delta = (int)seq.Slice(seq.Start, consumed).Length;
        BytesConsumed += delta;
        buffer = buffer[delta..];
        lastSequence = null;
    }

    public void AdvanceTo(SequencePosition consumed, SequencePosition examinned) =>
        AdvanceTo(consumed);
}