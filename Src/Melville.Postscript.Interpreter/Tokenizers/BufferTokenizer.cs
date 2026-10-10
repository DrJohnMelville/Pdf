using System;
using Melville.Postscript.Interpreter.Values;
using System.Buffers;
using Melville.Parsing.SequenceReaders;
using Melville.Postscript.Interpreter.Values.Interfaces;
using Melville.StreamInterfaces.ParsingBuffers;
using Melville.Parsing;

namespace Melville.Postscript.Interpreter.Tokenizers;


internal static class BufferTokenizer
{
    private static readonly FixedLengthOperatorTokenizer OneByteOperator = new (1);
    private static readonly FixedLengthOperatorTokenizer TwoByteOperator = new(2);
    private static readonly LiteralNameTokenizer LiteralName = new();
    private static readonly NameOrNumberTokenizer NameOrNumber = new();
    private static readonly SyntaxStringTokenizer SyntaxString = new();
    private static readonly HexStringTokenizer HexString = new();
    private static readonly Ascii85SpanTokenizer Ascii85 = new();
    private static readonly NeedMoreBytesTokenizer NeedBytes = new();
    private static readonly CommentSkipTokenizer CommentSkipper = new();
    private static readonly WhiteSpaceSkipper WhiteSpaceSkipper = new();

    private static SpanTokenizer SelectTokenStrategy(ReadOnlySpan<byte> bytes) => bytes switch
    {
        [] or
        [(byte)'<'] or
        [(byte)'>'] => NeedBytes,

        [(byte)'%',..] => CommentSkipper,
        [var ws, ..] when CharacterClassifier.IsWhitespace(ws) => WhiteSpaceSkipper,


        [(byte)'[', ..] or
        [(byte)']', ..] or
        [(byte)'{', ..] or
        [(byte)'}', ..] => OneByteOperator,
        [(byte)'<', (byte)'<', ..] or
        [(byte)'>', (byte)'>', ..] => TwoByteOperator,
        [(byte)'>', _, ..] => throw new PostscriptNamedErrorException(">> expected.", "syntaxerror"),

        [(byte)'(', ..] => SyntaxString,
        [(byte)'<', (byte)'~', ..] => Ascii85,
        [(byte)'<', ..] => HexString,

        [(byte)'/', ..] => LiteralName,
        _ => NameOrNumber
    };

    public static bool TryGetPostscriptToken(this ref SpanParsingBuffer spb, out PostscriptValue value) => SelectTokenStrategy(spb.Peek()).TryParse(ref spb, out value);
}
