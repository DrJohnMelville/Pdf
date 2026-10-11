using System;
using System.Buffers;
using System.Collections.Generic;
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
#warning make auto property
    public ParsingBuffer CodeSource => codeSource;

    private ParsingBuffer codeSource;
    private IAsyncReader reader;
    private IDisposable? source;

    /// <inheritdoc />
    public void Dispose()
    {
        codeSource.Dispose();
        (reader as IDisposable)?.Dispose();
        source?.Dispose();
    }

    /// <summary>
    /// Create a tokenizer from a stream.
    /// </summary>
    /// <param name="source">A stream containing the code to execute.</param>
    public Tokenizer(IAsyncReader reader)
    {
        this.reader = reader;
        codeSource = new ParsingBuffer(reader);
    }

    /// <summary>
    /// Create a tokenizer from a stream.
    /// </summary>
    /// <param name="source">A stream containing the code to execute.</param>
    public Tokenizer(Stream source) :
        this(source.AsStreamReader())
    {
        this.source = source;
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
            await codeSource.ReadBytesAsync(1).CA();
        }
        return value;
    }

    private PostscriptValue NextToken()
    {
        PostscriptValue value;
        while (!TryParseBuffer(out value))
        {
            codeSource.DangerousReadBytesSync();
        }
        return value;
    }

    private bool TryParseBuffer(out PostscriptValue postscriptValue)
    {
        EnsureTerminalCr();
        var buffer = new SpanParsingBuffer(codeSource.Peek());
        var parsed = (buffer.TryGetPostscriptToken(out postscriptValue));
        codeSource.Advance((int)buffer.RelativePosition);
        if (parsed) return true;
        if (!codeSource.DoneReadingSource) return false;
        postscriptValue = PostscriptValueFactory.CreateNull();
        return true;
    }

    private void EnsureTerminalCr()
    {
        if (codeSource.DoneReadingSource && 
            codeSource.Peek() is not ([] or [..,(byte)'\r']))
        {
            codeSource.AppendBytes((byte)'\r');
        }
    }

    public long PositionInStream => codeSource.RelativePosition;

    /// <inheritdoc/>
    public async IAsyncEnumerable<PostscriptValue> TokensAsync()
    {
        while ((await NextTokenAsync().CA()) is { IsNull: false } token)
        {
            if (!token.IsTrivia ) yield return token;
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