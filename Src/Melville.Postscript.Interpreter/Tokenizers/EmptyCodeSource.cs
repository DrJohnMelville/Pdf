using System;
using System.Buffers;
using System.IO.Pipelines;
using System.Threading.Tasks;
using Melville.INPC;

namespace Melville.Postscript.Interpreter.Tokenizers;

/// <summary>
/// A code source with no code in it.  Used for running pre-parsed type 4 functions
/// </summary>
[StaticSingleton]
public sealed partial class EmptyCodeSource : ICodeSource
{
    /// <inheritdoc/>
    public ValueTask<ReadResult> ReadAsync() => new(Read());

    /// <inheritdoc/>
    public ReadResult Read() => new(ReadOnlySequence<byte>.Empty, false, true);

    /// <inheritdoc/>
    public void AdvanceTo(SequencePosition consumed)
    {
    }

    /// <inheritdoc/>
    public void AdvanceTo(SequencePosition consumed, SequencePosition examined)
    {
    }
}