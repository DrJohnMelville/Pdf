using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Dynamic;
using System.IO;
using System.IO.Pipelines;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Melville.INPC;
using Melville.Parsing.Streams;
using Melville.Parsing.VariableBitEncoding;
using Melville.Postscript.Interpreter.Values;

namespace Melville.Postscript.Interpreter.Tokenizers;

/// <summary>
/// Synchronously or asynchronously reads tokens from a code source.
/// </summary>
public partial class Tokenizer : ITokenSource
{
    /// <inheritdoc />
    public ICodeSource CodeSource { get; }

    internal Tokenizer(ICodeSource codeSource)
    {
        CodeSource = codeSource;
    }

    /// <summary>
    /// Create a tokenizer from a stream.
    /// </summary>
    /// <param name="source">A stream containing the code to execute.</param>
    public Tokenizer(Stream source) :
        this(new PipeWrapper(PipeReader.Create(source)))
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
    public Tokenizer(Memory<byte> source) : this(new MemoryWrapper(source))
    {
    }

    private async ValueTask<PostscriptValue> NextTokenAsync()
    {
        while (true)
        {
            var buffer = await CodeSource.ReadAsync();
            if (TryParseBuffer(buffer, out var postscriptValue))
            {
                return postscriptValue;
            }
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
        if (TryParse(buffer.Buffer, out var result) ||
            TryFinalParse(buffer, out result))
        {
            postscriptValue = result;
            return true;
        }

        if (buffer.IsCompleted)
        {
            postscriptValue = PostscriptValueFactory.CreateNull();
            return true;
        }

        CodeSource.AdvanceTo(buffer.Buffer.Start, buffer.Buffer.End);
        postscriptValue = default;
        return false;
    }

    private bool TryParse(ReadOnlySequence<byte> buffer, out PostscriptValue value)
    {
        var reader = new SequenceReader<byte>(buffer);
        if (reader.TryGetPostscriptToken(out value))
        {
            CodeSource.AdvanceTo(reader.Position);
            return true;
        }

        return false;
    }

    private bool TryFinalParse(ReadResult buffer, out PostscriptValue result)
    {
        if (buffer.IsCompleted && buffer.Buffer.Length != 0)
            return TryParseWithAppendedCarriageReturn(buffer, out result);

        result = default;
        return false;
    }

    private bool TryParseWithAppendedCarriageReturn(ReadResult buffer, out PostscriptValue result) =>
        TryParseInSeparateSequence(
            buffer.Buffer.AppendCR(), buffer.Buffer, out result);

    private bool TryParseInSeparateSequence(
        ReadOnlySequence<byte> appendedSequence,
        ReadOnlySequence<byte> originalSequence, out PostscriptValue result)
    {
        var reader = new SequenceReader<byte>(appendedSequence);
        if (!reader.TryGetPostscriptToken(out result)) return false;

        var offset = appendedSequence.Slice(appendedSequence.Start, reader.Position).Length;
        CodeSource.AdvanceTo(originalSequence.GetPosition(offset));
        return true;
    }

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