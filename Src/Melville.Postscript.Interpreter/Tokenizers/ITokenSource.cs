using System.Collections.Generic;
using System.Threading.Tasks;
using Melville.Postscript.Interpreter.Values;

namespace Melville.Postscript.Interpreter.Tokenizers;

/// <summary>
/// This is a source of postscript tokens.  This could be a
/// parser over a stream or an enumeration of pre-tokenized values.
/// </summary>
public interface ITokenSource
{
    /// <summary>
    /// The ICodeSource reading bytes from
    /// </summary>
    ICodeSource CodeSource { get; }

    /// <summary>
    /// Get all the tokens as a syncronous enumerable.
    /// </summary>
    public IEnumerable<PostscriptValue> Tokens();
    /// <summary>
    /// An async enumeration of sll the tokens in a sou0rce.
    /// </summary>
    public IAsyncEnumerable<PostscriptValue> TokensAsync();
}