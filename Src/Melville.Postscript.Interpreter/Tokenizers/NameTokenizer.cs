using System;
using System.Buffers;
using Melville.INPC;
using Melville.Postscript.Interpreter.Values;
using Melville.StreamInterfaces.ParsingBuffers;

namespace Melville.Postscript.Interpreter.Tokenizers;

internal sealed class NameOrNumberTokenizer() : SpanTokenizer
{
    protected override bool TryFindLength(ReadOnlySpan<byte> source, out int length)
    {
        length = source.IndexOfAny(CharacterClassifier.DelimiterSearchValues);
        return length > 0;
    }
    protected override PostscriptValue DecodeSpan(ReadOnlySpan<byte> source)
    {
        return NumberTokenizer.TryDetectNumber(source, out var ret) ? ret :
            PostscriptValueFactory.CreateString(source, StringKind.Name);
    }

}

internal sealed class LiteralNameTokenizer : SpanTokenizer
{
    protected override bool TryFindLength(ReadOnlySpan<byte> source, out int length)
    {
        length = 1+source[1..].IndexOfAny(CharacterClassifier.DelimiterSearchValues);
        return length > 0;
    }
    protected override PostscriptValue DecodeSpan(ReadOnlySpan<byte> source) =>
        PostscriptValueFactory.CreateString(source[1..], StringKind.LiteralName);

}
