using System.Collections.Generic;
using System.Linq;
using Melville.INPC;
using Melville.Parsing.CountingReaders;
using Melville.Postscript.Interpreter.Values;

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
    public IByteSourceWithGlobalPosition CodeSource => EmptyCodeSource.Instance;

    /// <inheritdoc/>
    public IEnumerable<PostscriptValue> Tokens() => source;

    /// <inheritdoc/>
    public IAsyncEnumerable<PostscriptValue> TokensAsync() =>
        source.ToAsyncEnumerable();
}