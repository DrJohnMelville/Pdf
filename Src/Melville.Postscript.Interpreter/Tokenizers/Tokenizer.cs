using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipelines;
using System.Text;
using System.Threading.Tasks;
using Melville.INPC;
using Melville.Parsing.CountingReaders;
using Melville.Parsing.MultiplexSources;
using Melville.Parsing.PipeReaders;
using Melville.Parsing.Streams;
using Melville.Postscript.Interpreter.Values;
using Melville.StreamInterfaces;
using Melville.StreamInterfaces.Memory;
using Melville.StreamInterfaces.ParsingBuffers;
using Melville.StreamInterfaces.Streams;

namespace Melville.Postscript.Interpreter.Tokenizers;

/// <summary>
/// Synchronously or asynchronously reads tokens from a code source.
/// </summary>
public partial class Tokenizer : ITokenSource
{
    /// <summary>
    /// The parsing buffer that the Tokenizer is reading from.
    /// </summary>
   public ParsingBuffer CodeSource { get; set; }

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
    /// Create a tokenizer from a stream.
    /// </summary>
    /// <param name="source">A stream containing the code to execute.</param>
    [Obsolete("Prefer the IAsyncReader method for more efficency")]
    public Tokenizer(Stream source) :
        this(source.AsStreamReader())
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

    private static IByteSource PipeFromMemory(Memory<byte> source)
    {
        using var multiplexSource = MultiplexSourceFactory.Create(source);
        return multiplexSource.ReadPipeFrom(0);
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
        EnsureTerminalCr();
        var buffer = new SpanParsingBuffer(CodeSource.Peek());
        var parsed = (buffer.TryGetPostscriptToken(out postscriptValue));
        CodeSource.Advance((int)buffer.RelativePosition);
        if (parsed) return true;
        if (!CodeSource.DoneReadingSource) return false;
        postscriptValue = PostscriptValueFactory.CreateNull();
        return true;
    }

    private void EnsureTerminalCr()
    {
        if (CodeSource.DoneReadingSource && 
            CodeSource.Peek() is not ([] or [..,(byte)'\r']))
        {
            CodeSource.AppendBytes((byte)'\r');
        }
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