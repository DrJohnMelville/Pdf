﻿using System;
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

namespace Melville.Postscript.Interpreter.Tokenizers;

/// <summary>
/// Synchronously or asynchronously reads tokens from a code source.
/// </summary>
public partial class Tokenizer : ITokenSource
{
    /// <inheritdoc />
    [FromConstructor] public IByteSource CodeSource { get; }

    /// <inheritdoc />
    public void Dispose() => CodeSource.Dispose();

    /// <summary>
    /// Create a tokenizer from a stream.
    /// </summary>
    /// <param name="source">A stream containing the code to execute.</param>
    public Tokenizer(Stream source) :
        this(MultiplexSourceFactory.SingleReaderForStream(source, false))
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
        PipeFromMemory(source))
    {
    }

    private static IByteSource PipeFromMemory(Memory<byte> source)
    {
        using var multiplexSource = MultiplexSourceFactory.Create(source);
        return multiplexSource.ReadPipeFrom(0);
    }

    private async ValueTask<PostscriptValue> NextTokenAsync()
    {
        while (true)
        {
            var buffer = await CodeSource.ReadAsync();
            if (TryParseBuffer(buffer, out var postscriptValue)) return postscriptValue;
            CodeSource.MarkSequenceAsExamined();
        }
    }

    private PostscriptValue NextToken()
    {
        while (true)
        {
            var buffer = CodeSource.Read();
            if (TryParseBuffer(buffer, out var postscriptValue))
            {
                return postscriptValue;
            }
        }
    }

    private bool TryParseBuffer(ReadResult buffer, out PostscriptValue postscriptValue)
    {
        if (TryParse(buffer, out postscriptValue)) return true;
        if (!buffer.IsCompleted) return false;
        postscriptValue = PostscriptValueFactory.CreateNull();
        return true;
    }

    private bool TryParse(ReadResult source, out PostscriptValue value)
    {
        var reader = new SequenceReader<byte>(AppendCrIfFinal(source));
        if (reader.TryGetPostscriptToken(out value))
        {
            CodeSource.AdvanceTo(source.Buffer.GetPosition(reader.Consumed));
            return true;
        }
        else
        {
            CodeSource.MarkSequenceAsExamined();
        }
        return false;
    }

    private static ReadOnlySequence<byte> AppendCrIfFinal(ReadResult source) => 
        source.IsCompleted?source.Buffer.AppendCR():source.Buffer;

    /// <inheritdoc/>
    public async IAsyncEnumerable<PostscriptValue> TokensAsync()
    {
        while ((await NextTokenAsync()) is { IsNull: false } token)
        {
            yield return token;
        }
    }

    /// <inheritdoc/>
    public IEnumerable<PostscriptValue> Tokens()
    {
        while (NextToken() is { IsNull: false } token)
        {
            yield return token;
        }
    }
}