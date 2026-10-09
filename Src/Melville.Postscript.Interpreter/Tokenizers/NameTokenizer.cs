using System;
using System.Buffers;
using Melville.INPC;
using Melville.Postscript.Interpreter.Values;
using Melville.StreamInterfaces.ParsingBuffers;

namespace Melville.Postscript.Interpreter.Tokenizers;

internal readonly struct NameTokenizer(StringKind kind)
{
    public bool TryParse(ref SpanParsingBuffer input, out PostscriptValue value)
    {
        var end = input.Peek().IndexOfAny(CharacterClassifier.DelimiterChars());
        return end switch
        {
            < 0 => default(PostscriptValue).AsFalseValue(out value),
            0 => PostscriptValueFactory.CreateString("", kind).AsTrueValue(out value),
            _ => CreateName(ref input, end, out value)
        };
    }

    private bool CreateName(ref SpanParsingBuffer input, int length, out PostscriptValue result)
    {
        var buffer = input.Peek()[..length];

        var ret = (kind == StringKind.Name && NumberTokenizer.TryDetectNumber(buffer, out result)) ||
            PostscriptValueFactory.CreateString(buffer, kind).AsTrueValue(out result);

        input.Advance(length);
        return ret;
    }
}
