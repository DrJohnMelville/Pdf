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
/// This is a source of postscrupt tokens.  This could be a
/// parser over a stream or an enumeration of pre-tokenized values.
/// </summary>
public interface ITokenSource
{
    /// <summary>
    /// The ICodeSource reading bytes from
    /// </summary>
    ICodeSource CodeSource { get; }

    /// <summary>
    /// Get the next token in an async method.
    /// </summary>
    /// <returns>The next token or null if at the end of the stream</returns>
    ValueTask<PostscriptValue> NextTokenAsync();
    /// <summary>
    /// Get the next token in an async method.
    /// </summary>
    /// <returns>The next token or null if at the end of the stream</returns>
    PostscriptValue NextToken();
    /// <summary>
    /// A synchronous enumeration of all the tokens in the source.
    /// </summary>
    public IEnumerable<PostscriptValue> Tokens();
    /// <summary>
    /// An async enumeration of sll the tokens in a sou0rce.
    /// </summary>
    public IAsyncEnumerable<PostscriptValue> TokensAsync();
}

public partial class Tokenizer : ITokenSource
{
    public ICodeSource CodeSource { get; }

    internal Tokenizer(ICodeSource codeSource)
    {
        CodeSource = codeSource;
    }

    public Tokenizer(Stream source) :
        this(new PipeWrapper(PipeReader.Create(source)))
    {
    }

    public Tokenizer(string source) : this(Encoding.ASCII.GetBytes(source))
    {
    }

    public Tokenizer(Memory<byte> source) : this(new MemoryWrapper(source))
    {
    }

    public async ValueTask<PostscriptValue> NextTokenAsync()
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

    public PostscriptValue NextToken()
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

    public async IAsyncEnumerable<PostscriptValue> TokensAsync()
    {
        while ((await NextTokenAsync()) is { IsNull: false } token)
        {
            yield return token;
        }
    }

    public IEnumerable<PostscriptValue> Tokens()
    {
        while (NextToken() is { IsNull: false } token)
        {
            yield return token;
        }
    }
}