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
            length = source.IndexOf((byte)'>');
            return length > 0;
        }
        protected override PostscriptValue DecodeSpan(ReadOnlySpan<byte> source)
        {
            var input = new SpanParsingBuffer(source[1..^1]);
            using var output = new SpanWriter(input.RemainingLength / 2);
            while (input.TryGetNext(CharacterClassifier.HexDigits, out var high))
            {
                if (!input.TryGetNext(CharacterClassifier.HexDigits, out var low))
                    low = (byte)'0';

                output.Push((byte)(CharacterClassifier.ValueFromDigit(high) << 4 | CharacterClassifier.ValueFromDigit(low)));
            }
            return PostscriptValueFactory.CreateString(output.Value, StringKind.String);
        }
    }
}