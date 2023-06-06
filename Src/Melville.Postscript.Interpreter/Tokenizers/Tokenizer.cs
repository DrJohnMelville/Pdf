using System;
using System.Buffers;
using System.IO;
using System.IO.Pipelines;
using System.Text;
using System.Threading.Tasks;
using Melville.INPC;
using Melville.Postscript.Interpreter.Values;

namespace Melville.Postscript.Interpreter.Tokenizers;

internal partial class Tokenizer
{
    [FromConstructor] private PipeReader source;

    public Tokenizer(Stream source) : this(PipeReader.Create(source))
    {
    }
    public Tokenizer(string source) : this(new MemoryStream(Encoding.ASCII.GetBytes(source)))
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
            return TryParseWithAppendedSpace(buffer, out result);

        result = default;
        return false;

    }

    private bool TryParseWithAppendedSpace(ReadResult buffer, out PostscriptValue result)
    {
        var scratch = ArrayPool<byte>.Shared.Rent((int)buffer.Buffer.Length + 1);
        try
        {
            return TryParseInSeparateSequence(CreateAppendedSequence(buffer, scratch), buffer.Buffer, out result);
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(scratch);
        }
    }

    private bool TryParseInSeparateSequence(
        ReadOnlySequence<byte> appendedSequence, 
        ReadOnlySequence<byte> originalSequence, out PostscriptValue result)
    {
        var reader = new SequenceReader<byte>(appendedSequence);
        if (!reader.TryGetPostscriptToken(out result)) return false;
        
        var offset = appendedSequence.Slice(appendedSequence.Start, reader.Position).Length;
        source.AdvanceTo(originalSequence.Start, originalSequence.GetPosition(offset));
        return true;
    }

    private static ReadOnlySequence<byte> CreateAppendedSequence(
        ReadResult buffer, byte[] scratch)
    {
        var priorLength = (int)buffer.Buffer.Length;
        buffer.Buffer.CopyTo(scratch.AsSpan(0, priorLength));
        scratch[buffer.Buffer.Length] = 32;
        var seq = new ReadOnlySequence<byte>(scratch, 0, priorLength+1);
        return seq;
    }
}

internal static class CharacterClassifier
{
    public static bool IsWhitespace(byte character) =>
        character is 0 or 9 or 10 or 12 or 13 or 32;
}