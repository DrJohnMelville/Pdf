using System;
using Melville.Postscript.Interpreter.Values;
using System.Buffers;
using Melville.Parsing.SequenceReaders;
using Melville.Postscript.Interpreter.Values.Interfaces;
using Melville.StreamInterfaces.ParsingBuffers;
using Melville.Parsing;

namespace Melville.Postscript.Interpreter.Tokenizers;

internal static class SequenceTokenizer
{
    private static readonly FixedLengthOperatorTokenizer OneByteOperator = new (1);
    private static readonly FixedLengthOperatorTokenizer TwoByteOperator = new(2);
    private static readonly LiteralNameTokenizer LiteralName = new();
    private static readonly NameOrNumberTokenizer NameOrNumber = new();
    private static readonly SyntaxStringTokenizer SyntaxString = new();
    private static readonly HexStringTokenizer HexString = new();
    private static readonly Ascii85SpanTokenizer Ascii85 = new();

    public static bool TryGetPostscriptToken(
        this ref SequenceReader<byte> reader, out PostscriptValue value)
    {
        if (!reader.TryPeekNextNonComment(out var firstChar))
            return default(PostscriptValue).AsFalseValue(out value);
        var data = reader.UnreadSequence.ToArray();
        var spb = new SpanParsingBuffer(data);

        try
        {
            return spb.Peek() switch
            {
                [] or
                [(byte)'<'] or
                [(byte)'>'] => default(PostscriptValue).AsFalseValue(out value),

                [(byte)'[', ..] or
                [(byte)']', ..] or
                [(byte)'{', ..] or
                [(byte)'}', ..] => OneByteOperator.TryParse(ref spb, out value),
                [(byte)'<',(byte)'<', ..] or
                [(byte)'>', (byte)'>', ..] => TwoByteOperator.TryParse(ref spb, out value),
                [(byte)'>', _, ..] => throw new PostscriptNamedErrorException(">> expected.", "syntaxerror"),

                [(byte)'(', ..] => SyntaxString.TryParse(ref spb, out value),
                [(byte)'<', (byte)'~', ..] => Ascii85.TryParse(ref spb, out value),
                //[(byte)'`', (byte)'~', ..] => ParseString<Ascii85StringDecoder, byte>(ref reader.WithAdvance(2), out value) &&
                //                                 reader.TryAdvance(1),
                [(byte)'<', ..] => ParseString<HexStringDecoder, byte>(ref reader.WithAdvance(1), out value),

                [(byte)'/', ..] => LiteralName.TryParse(ref spb, out value),
                _ => NameOrNumber.TryParse(ref spb, out value)

            };
        }
        finally 
        {
            if (spb.RelativePosition > 0)
            {
                reader.Advance(spb.RelativePosition);
            }
        }
    }

    private static bool ParseString<T, TState>(ref SequenceReader<byte> reader, out PostscriptValue value) where T : IStringDecoder<TState>, new()
           where TState : new() =>
        new StringTokenizer<T, TState>().Parse(ref reader, out value);
}
