using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Melville.INPC;
using Melville.Parsing.CountingReaders;
using Melville.Postscript.Interpreter.Values;
using Melville.StreamInterfaces;
using Melville.StreamInterfaces.ParsingBuffers;

namespace Melville.Postscript.Interpreter.Tokenizers;

/// <summary>
/// A class that feeds pre-tokenized code into the postscript engine
/// </summary>
public partial class PretokenizedSource : ITokenSource
{
    /// <summary>
    /// The source of the tokens
    /// </summary>
    [FromConstructor] private IEnumerable<PostscriptValue> source;

    /// <inheritdoc/>
    public ParsingBuffer CodeSource { get; set; } = new ParsingBuffer(new EmptyAsyncReader());

    /// <inheritdoc/>
    public IEnumerable<PostscriptValue> Tokens() => source;

    /// <inheritdoc/>
    public IAsyncEnumerable<PostscriptValue> TokensAsync() =>
        source.ToAsyncEnumerable();

    /// <inheritdoc />
    public void Dispose()
    {
    }
}

internal class EmptyAsyncReader : IAsyncReader, ISyncReader
{
    public int Read(Span<byte> buffer) => 0;

    public ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellation = default) =>
        ValueTask.FromResult(0);
}