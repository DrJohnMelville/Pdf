using System;
using System.Buffers;
using System.IO.Pipelines;
using System.Threading;
using System.Threading.Tasks;
using Melville.INPC;
using Melville.Parsing.CountingReaders;

namespace Melville.Postscript.Interpreter.Tokenizers;

/// <summary>
/// A code source with no code in it.  Used for running pre-parsed type 4 functions
/// </summary>
[StaticSingleton]
public sealed partial class EmptyCodeSource : IByteSource
{
    /// <inheritdoc/>
    public ValueTask<ReadResult> ReadAsync() =>
        new(singletonResult);

    /// <inheritdoc />
    public ReadResult Rea() => singletonResult;


    /// <inheritdoc/>
    public void AdvanceTo(SequencePosition consumed)
    {
    }

    /// <inheritdoc/>
    public void AdvanceTo(SequencePosition consumed, SequencePosition examined)
    {
    }


    /// <inheritdoc />
    public bool TryRead(out ReadResult result)
    { 
        result = singletonResult;
        return true;
    }

    private static ReadResult singletonResult = 
        new(ReadOnlySequence<byte>.Empty, false, true);


    /// <inheritdoc />
    public void MarkSequenceAsExamined()
    {
    }

    /// <inheritdoc />
    public long Position { get; private set; }

    /// <inheritdoc />
    public void Dispose()
    {
    }

    /// <inheritdoc />
    public void RemapCurrentPosition(long newPosition) => Position = newPosition;
}