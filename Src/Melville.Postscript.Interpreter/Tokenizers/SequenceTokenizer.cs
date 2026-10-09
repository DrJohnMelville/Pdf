using System;
using Melville.Postscript.Interpreter.Values;
using System.Buffers;
using Melville.Parsing.SequenceReaders;
using Melville.Postscript.Interpreter.Values.Interfaces;
using Melville.StreamInterfaces.ParsingBuffers;

namespace Melville.Postscript.Interpreter.Tokenizers;

internal static class SequenceTokenizer
{
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
                [(byte)'}', ..] => TryCopyLiteral(1, ref spb, out value),
                [(byte)'<',(byte)'<', ..] or
                [(byte)'>', (byte)'>', ..] => TryCopyLiteral(2, ref spb, out value),
                [(byte)'>', _, ..] => throw new PostscriptNamedErrorException(">> expected.", "syntaxerror"),

                [(byte)'(', ..] => ParseString<SyntaxStringDecoder, int>(ref reader.WithAdvance(1), out value),
                [(byte)'<', (byte)'~', ..] => ParseString<Ascii85StringDecoder, byte>(ref reader.WithAdvance(2), out value) &&
                                                 reader.TryAdvance(1),
                [(byte)'<', ..] => ParseString<HexStringDecoder, byte>(ref reader.WithAdvance(1), out value),

                [(byte)'/', ..] => new NameTokenizer(StringKind.LiteralName)
                        .TryParse(ref spb.WithAdvance(), out value),
                _ => new NameTokenizer(StringKind.Name).TryParse(ref spb, out value)

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

   private static bool TryCopyLiteral(
        int length, ref SpanParsingBuffer reader, out PostscriptValue value)
    {
        value = PostscriptValueFactory.CreateString(reader.Peek()[..length], StringKind.Name);
        reader.Advance(length);
        return true;
    }

    private static bool ParseString<T, TState>(ref SequenceReader<byte> reader, out PostscriptValue value) where T : IStringDecoder<TState>, new()
           where TState : new() =>
        new StringTokenizer<T, TState>().Parse(ref reader, out value);
}