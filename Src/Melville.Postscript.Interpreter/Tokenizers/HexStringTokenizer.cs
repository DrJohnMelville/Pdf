using Melville.Parsing;
using Melville.Postscript.Interpreter.Values;
using Melville.StreamInterfaces.ParsingBuffers;
using System;

namespace Melville.Postscript.Interpreter.Tokenizers
{
    internal sealed class HexStringTokenizer : SpanTokenizer
    {

        protected override bool TryFindLength(ReadOnlySpan<byte> source, out int length)
        {
            length = 1+source.IndexOf((byte)'>');
            return length > 0;
        }
        protected override PostscriptValue DecodeSpan(ReadOnlySpan<byte> source)
        {
            var input = new SpanParsingBuffer(source[1..^1]);
            using var output = new SpanWriter(input.RemainingLength / 2);
            while (input.TryGetNext(CharacterClassifier.HexDigitSearchValues, out var high))
            {
                if (!input.TryGetNext(CharacterClassifier.HexDigitSearchValues, out var low))
                    low = (byte)'0';

                output.Push(CharacterClassifier.ByteFromHexChars(high, low));
            }
            return PostscriptValueFactory.CreateString(output.Value, StringKind.String);
        }
    }
}