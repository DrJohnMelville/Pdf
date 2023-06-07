using Melville.Postscript.Interpreter.Values;
using System.Buffers;
using System.Diagnostics;
using System.Net.Http.Headers;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Melville.Postscript.Interpreter.Tokenizers;

internal static class SequenceTokenizer
{
    public static bool TryGetPostscriptToken(
        this ref SequenceReader<byte> reader, out PostscriptValue value)
    {
        if (!CommentSkipper.SkipWhiteSpace(ref reader, out var firstChar))
            return default(PostscriptValue).AsFalseValue(out value);
        return firstChar switch
        {
            (int)'/' => new NameTokenizer(StringKind.LiteralName)
                .TryParse(ref reader.WithAdvance(), out value),
            _ => new NameTokenizer(StringKind.Name).TryParse(ref reader, out value)
        };
    }
}