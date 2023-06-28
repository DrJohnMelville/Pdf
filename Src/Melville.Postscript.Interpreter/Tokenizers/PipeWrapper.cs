using System;
using System.IO.Pipelines;
using System.Threading.Tasks;
using Melville.INPC;

namespace Melville.Postscript.Interpreter.Tokenizers;

/// <summary>
/// Access a pipe reader as an ICodeSource
/// </summary>
public partial class PipeWrapper : ICodeSource
{
    /// <summary>
    /// The PipeReader to get data from.
    /// </summary>
    [FromConstructor] private readonly PipeReader reader;

    /// <inheritdoc />
    public ValueTask<ReadResult> ReadAsync()
    {
        return reader.ReadAsync();
    }

    /// <inheritdoc />
    public ReadResult Read()
    {
        throw new NotSupportedException(
            "A Pipewrapper must be read using the async method");
    }

    /// <inheritdoc />
    public void AdvanceTo(SequencePosition consumed) => reader.AdvanceTo(consumed);

    /// <inheritdoc />
    public void AdvanceTo(SequencePosition consumed, SequencePosition examined) =>
        reader.AdvanceTo(consumed, examined);
}