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
