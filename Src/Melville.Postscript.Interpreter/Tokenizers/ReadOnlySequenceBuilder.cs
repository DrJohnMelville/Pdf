using System;
using System.Buffers;

namespace Melville.Postscript.Interpreter.Tokenizers;

internal ref struct ReadOnlySequenceBuilder<T>
{
    private ReadOnlyChunk? first;
    private ReadOnlyChunk? last;

    public ReadOnlySequenceBuilder()
    {
        first = last = null;
    }

    public ReadOnlySequenceBuilder(ReadOnlySequence<T> sequence) : this()
    {
        Append(sequence);
    }

    public void Append(ReadOnlySequence<T> sequence)
    {
        var pos = sequence.Start;
        while (sequence.TryGet(ref pos, out ReadOnlyMemory<T> mem, true))
        {
            Append(mem);
        }
    }

    public void Append(ReadOnlyMemory<T> memory)
    {
        if (last == null)
        {
            first = last = new ReadOnlyChunk(memory);
        }
        else
        {
            last = last.Append(memory);
        }
    }

    public ReadOnlySequence<T> GetSequence() =>
        first is null || last is null
            ? new ReadOnlySequence<T>()
            : new ReadOnlySequence<T>(first, 0, last, last.Memory.Length);

    private sealed class ReadOnlyChunk : ReadOnlySequenceSegment<T>
    {
        public ReadOnlyChunk(ReadOnlyMemory<T> memory)
        {
            Memory = memory;
        }

        public ReadOnlyChunk Append(ReadOnlyMemory<T> memory)
        {
            var nextChunk = new ReadOnlyChunk(memory)
            {
                RunningIndex = RunningIndex + Memory.Length
            };

            Next = nextChunk;
            return nextChunk;
        }
    }
}