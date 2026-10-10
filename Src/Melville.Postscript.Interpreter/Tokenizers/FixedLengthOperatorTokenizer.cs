using Melville.Postscript.Interpreter.Values;
using System;

namespace Melville.Postscript.Interpreter.Tokenizers;

internal sealed class FixedLengthOperatorTokenizer(int fixedLength) : SpanTokenizer
{
    protected override bool TryFindLength(ReadOnlySpan<byte> source, out int length)
    {
        length = fixedLength;
        return source.Length >= fixedLength;
    }
    protected override PostscriptValue DecodeSpan(ReadOnlySpan<byte> source) =>
        PostscriptValueFactory.CreateString(source, StringKind.Name);

}