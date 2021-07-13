using System;
using System.Buffers;
using System.IO.Pipelines;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Parsing.ParserContext;

namespace Melville.Pdf.LowLevel.Parsing.ObjectParsers
{
    public static class NextTokenFinder
    {
        public static async ValueTask SkipToNextToken(ParsingSource source)
        {
            do {} while (source.ShouldContinue(SkipToNextToken2(await source.ReadAsync())));
        }

        private static (bool Success, SequencePosition Position) SkipToNextToken2(ReadResult source)
        {
            var reader = new SequenceReader<byte>(source.Buffer);
            return (SkipToNextToken(ref reader), reader.Position);
        }

        public static bool SkipToNextToken(ref SequenceReader<byte> input)
        {
            if (!SkipToNextToken(ref input, out _)) return false;
            input.Rewind(1);
            return true;
        }

        public static bool SkipToNextToken(ref SequenceReader<byte> input, out byte firstByte)
        {
            while (true)
            {
                if (!input.TryRead(out firstByte)) return false;
                switch (CharClassifier.Classify(firstByte))
                {
                    case CharacterClass.White:
                        break;
                    case CharacterClass.Delimiter when firstByte == (byte) '%':
                        if (!input.TrySkipToEndOfLineMarker()) return false;
                        break;
                    default:
                        return true;
                }
            }
        }
        public static bool TryCheckToken(
            this ref SequenceReader<byte> input, byte[] template, out bool result)
        {
            result = false;
            foreach (var expected in template)
            {
                if (!input.TryRead(out var actual)) return false;
                if (expected != actual) return true;
            }
            result = true;
            return true;
        }
    }
}