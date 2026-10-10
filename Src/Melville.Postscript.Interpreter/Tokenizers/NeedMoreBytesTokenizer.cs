using Melville.Postscript.Interpreter.Values;
using System;

namespace Melville.Postscript.Interpreter.Tokenizers;

internal class NeedMoreBytesTokenizer : SpanTokenizer
{
    protected override PostscriptValue DecodeSpan(ReadOnlySpan<byte> source) => throw new NotSupportedException("This class never reports a length");

    protected override bool TryFindLength(ReadOnlySpan<byte> source, out int length) =>
        0.AsFalseValue(out length);
}
