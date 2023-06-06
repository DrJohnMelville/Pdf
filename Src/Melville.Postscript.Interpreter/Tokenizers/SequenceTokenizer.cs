using Melville.Postscript.Interpreter.Values;
using System.Buffers;
using System.Net.Http.Headers;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Melville.Postscript.Interpreter.Tokenizers;

internal static class SequenceTokenizer
{
    public static bool TryGetPostscriptToken(this ref SequenceReader<byte> reader, out PostscriptValue value)
    {
        value = default;
        if (!reader.TryPeek(out var firstChar)) return false;
        reader.TryPeek(1, out var secondChar);

        return (firstChar, secondChar) switch
        {
            ((>= (int)'0' and <= (int)'9') or 
                (int)'.' or 
                (int)'-' or 
                (int)'+', _) => 
                new NumberTokenizer(10).TryParse(ref reader, out value),
            _ => false
        };
    }
}