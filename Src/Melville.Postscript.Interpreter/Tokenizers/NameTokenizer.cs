using System;
using System.Buffers;
using System.Diagnostics;
using System.Reflection.Metadata;
using System.Text.RegularExpressions;
using Melville.INPC;
using Melville.Postscript.Interpreter.Values;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Melville.Postscript.Interpreter.Tokenizers;

internal readonly partial struct NameTokenizer
{
    [FromConstructor] private readonly StringKind kind;

    public bool TryParse(ref SequenceReader<byte> input, out PostscriptValue value)
    {
        return TryGetNameByteSequence(ref input, out var buffer)
            ? TryCreateNumber(buffer, out value)
            : default(PostscriptValue).AsFalseValue(out value);
    }

    private static bool TryGetNameByteSequence(ref SequenceReader<byte> input, out ReadOnlySpan<byte> buffer)
        => input.TryReadToAny(out buffer, CharacterClassifier.DelimiterChars(), false);

    private bool TryCreateNumber(
        in ReadOnlySpan<byte> buffer, out PostscriptValue value)
    {
        if (!(kind == StringKind.Name && buffer.Length > 0 &&
              NumberTokenizer.TryDetectNumber(buffer, out value)))
            value = PostscriptValueFactory.CreateString(buffer, kind);
        return true;
    }
}