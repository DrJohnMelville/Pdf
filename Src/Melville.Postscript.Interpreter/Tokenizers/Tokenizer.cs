using Melville.Postscript.Interpreter.Values;
using Melville.StreamInterfaces;
using Melville.StreamInterfaces.Memory;
using Melville.StreamInterfaces.ParsingBuffers;
using Melville.StreamInterfaces.Streams;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Melville.Postscript.Interpreter.Tokenizers;

/// <summary>
/// Synchronously or asynchronously reads tokens from a code source.
/// </summary>
public partial class Tokenizer : ITokenSource
{
    /// <summary>
    /// The parsing buffer that the Tokenizer is reading from.
    /// </summary>
   public ParsingBuffer CodeSource { 
        get; 
        set 
        {
            field?.Dispose();
            field = value;
        } 
    }

    /// <inheritdoc />
    public void Dispose()
    {
        CodeSource.Dispose();
    }

    /// <summary>
    /// Create a tokenizer from a ParsingBuffer
    /// </summary>
    /// <param name="codeSource"></param>
    public Tokenizer(ParsingBuffer codeSource) => CodeSource = codeSource;

    /// <summary>
    /// Create a tokenizer from an AsyncReader.
    /// </summary>
    /// <param name="reader">An IAsyncReader containing the code to execute.</param>
    public Tokenizer(IAsyncReader reader): this(new ParsingBuffer(reader))
    {
    }

    /// <summary>
    /// Create a tokenizer from a string.
    /// </summary>
    /// <param name="source">The string containing the code to execute.</param>
    public Tokenizer(string source) : this(Encoding.ASCII.GetBytes(source))
    {
    }

    /// <summary>
    /// Create a tokenizer from a block of Memory.
    /// </summary>
    /// <param name="source">The code to execute.</param>
    public Tokenizer(Memory<byte> source) : this(
        new MemoryReader(source))
    {
    }

    private async ValueTask<PostscriptValue> NextTokenAsync()
    {
        PostscriptValue value;
        while (!TryParseBuffer(out value))
        {
            await CodeSource.ReadBytesAsync(1).CA();
        }
        return value;
    }

    private PostscriptValue NextToken()
    {
        PostscriptValue value;
        while (!TryParseBuffer(out value))
        {
            CodeSource.DangerousReadBytesSync();
        }
        return value;
    }

    private bool TryParseBuffer(out PostscriptValue postscriptValue)
    {
        var span = CodeSource.Peek();
        if (span.Length > 0 && CodeSource.DoneReadingSource && span.IndexOfAny(CharacterClassifier.WhitespaceSearchValues) < 0)
        {
            Span<byte> sp2 = stackalloc byte[span.Length + 1];
            span.CopyTo(sp2);
            sp2[^1] = (byte)'\n';
            return InnerTryParseBuffer(sp2, out postscriptValue);
        }
        return InnerTryParseBuffer(span, out postscriptValue);
    }

    private bool InnerTryParseBuffer(ReadOnlySpan<byte> span, out PostscriptValue postscriptValue)
    {
        var buffer = new SpanParsingBuffer(span);
        var parsed = (buffer.TryGetPostscriptToken(out postscriptValue));
        CodeSource.Advance(Math.Min((int)buffer.RelativePosition, CodeSource.CurrentLength)); // there may be an extra character on the end.
        if (parsed) return true;
        if (!CodeSource.DoneReadingSource) return false;
        postscriptValue = PostscriptValueFactory.CreateNull();
        return true;
    }

    /// <summary>
    /// The current location within the code being executed.
    /// </summary>
    public long PositionInStream => CodeSource.RelativePosition;

    /// <inheritdoc/>
    public async IAsyncEnumerable<PostscriptValue> TokensAsync()
    {
        while ((await NextTokenAsync().CA()) is { IsNull: false } token)
        {
            if (!token.IsTrivia)
                yield return token;
        }
    }

    /// <inheritdoc/>
    public IEnumerable<PostscriptValue> Tokens()
    {
        while (NextToken() is { IsNull: false } token)
        {
            if (!token.IsTrivia) yield return token;
        }
    }
}