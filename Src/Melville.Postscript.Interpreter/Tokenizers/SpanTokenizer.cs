using Melville.Parsing;
using Melville.Postscript.Interpreter.Values;
using Melville.StreamInterfaces.ParsingBuffers;
using System;

namespace Melville.Postscript.Interpreter.Tokenizers;

internal abstract class SpanTokenizer
{

    public bool TryParse(ref SpanParsingBuffer source, out PostscriptValue value)
    {
        if (!TryFindLength(source.Peek(), out var length))
            return default(PostscriptValue).AsFalseValue(out value);
        value = DecodeSpan(source.Peek(length));
        source.Advance(length);
        return true;
    }

    protected abstract bool TryFindLength(ReadOnlySpan<byte> source, out int length);
    protected abstract PostscriptValue DecodeSpan(ReadOnlySpan<byte> source);
}

internal sealed class Ascii85SpanTokenizer : SpanTokenizer
{
    protected override bool TryFindLength(ReadOnlySpan<byte> source, out int length)
    {
        length = source.IndexOf("~>"u8) +2;
        return length > 2;
    }
    protected override PostscriptValue DecodeSpan(ReadOnlySpan<byte> source)
    {
        var input = new SpanParsingBuffer(source[2..^2]);
        var output = new SpanWriter(input.RemainingLength);

        ReadBytes(ref input, ref output);
        return PostscriptValueFactory.CreateString(output.Value, StringKind.String);
    }

    private static void ReadBytes(ref SpanParsingBuffer input, ref SpanWriter output)
    {
        Span<byte> outputBuff = stackalloc byte[4];
        while (input.TryGetUInt8(out var firstByte))
        {
            if (firstByte == (int)'z')
            {
                outputBuff.Clear();
                WriteToBuff(ref output, outputBuff, 4);
            }
            else
            {
                var quint = new Ascii85Quintuple();
                quint.AddByte(firstByte);
                for (int i = 0; i < 4; i++)
                {
                    if (!input.TryGetUInt8(out var next))
                    {
                        var len = quint.WriteDecodedBytesPartial(outputBuff, i + 1);
                        WriteToBuff(ref output, outputBuff, len);
                        return;
                    }
                    quint.AddByte(next);
                }
                quint.WriteDecodedBytes(outputBuff, 4);
                WriteToBuff(ref output, outputBuff, 4);
            }
        }
    }

    private static void WriteToBuff(ref SpanWriter output, scoped Span<byte> outputBuff, int len)
    {
        for (int i = 0; i < len; i++)
        {
            output.Push(outputBuff[i]);
        }
    }
}