using Melville.Postscript.Interpreter.Values;
using System;

namespace Melville.Postscript.Interpreter.Tokenizers;

internal sealed class LiteralNameTokenizer : SpanTokenizer
{
    protected override bool TryFindLength(ReadOnlySpan<byte> source, out int length)
    {
        length = 1 + source[1..].IndexOfAny(CharacterClassifier.DelimiterSearchValues);
        return length > 0;
    }
    protected override PostscriptValue DecodeSpan(ReadOnlySpan<byte> source) =>
        PostscriptValueFactory.CreateString(source[1..], StringKind.LiteralName);

}
