using Melville.Postscript.Interpreter.Values;
using System;
using System.Diagnostics;

namespace Melville.Postscript.Interpreter.Tokenizers;

internal class WhiteSpaceSkipper : SpanTokenizer
{
    protected override bool TryFindLength(ReadOnlySpan<byte> source, out int length)
    {
        Debug.Assert(CharacterClassifier.IsWhitespace(source[0]));
        length = source.IndexOfAnyExcept(CharacterClassifier.WhitespaceSearchValues);
        if (length < 0) length = source.Length;
        return true;
        //A whitespace "token" is unique that you break it at any point and you get two whitespace tokens.
        //Thus we eagerly discard the firs half of any whitespace we find, even if we are not sure that it is
        // the end because if we can read all the way to the end of a ParseBuffer then we get a free reset
        // to the beginning 
    }
    protected override PostscriptValue DecodeSpan(ReadOnlySpan<byte> source) => PostscriptValueFactory.CreateTrivia();

}
