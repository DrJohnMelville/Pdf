using System;
using System.Buffers;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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

public static class SynchronousTokenizer
{
    public static IEnumerable<PostscriptValue> Tokenize(Memory<byte> input)
    {
        var seq = input.AppendCR();
        while (ReadFrom(ref seq, out var token))
            yield return token;
    }

    private static bool ReadFrom(ref ReadOnlySequence<byte> seq, out PostscriptValue token)
    {
        var reader = new SequenceReader<byte>(seq);
        var ret = reader.TryGetPostscriptToken(out token);
        seq = seq.Slice(reader.Consumed);
        return ret;
    }
}

internal partial class AsynchronousTokenizer: IAsyncEnumerable<PostscriptValue>
{
    [FromConstructor] private PipeReader source;

    public AsynchronousTokenizer(Stream source) : this(PipeReader.Create(source))
    {
    }

    public async ValueTask<PostscriptValue> NextTokenAsync()
    {
        while (true)
        {
            var buffer = await source.ReadAsync();
            if (TryParse(buffer.Buffer, out var result) || 
                TryFinalParse(buffer, out result)) return result;
            if (buffer.IsCompleted) return PostscriptValueFactory.CreateNull();
        }
    }

    private bool TryParse(ReadOnlySequence<byte> buffer, out PostscriptValue value)
    {
        var reader = new SequenceReader<byte>(buffer);
        if (reader.TryGetPostscriptToken(out value))
        {
            source.AdvanceTo(reader.Position);
            return true;
        }
        source.AdvanceTo(buffer.Start, buffer.End);
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
        source.AdvanceTo(originalSequence.GetPosition(offset));
        return true;
    }

    public async IAsyncEnumerator<PostscriptValue> GetAsyncEnumerator(
        CancellationToken cancellationToken = new CancellationToken())
    {
        while ((await NextTokenAsync()) is  {IsNull:false} token)
        {
            yield return token;
        }
    }
}