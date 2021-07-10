using System.Buffers;
using System.IO.Pipelines;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Parsing.ParserContext;

namespace Melville.Pdf.LowLevel.Parsing
{
    public static class NextTokenFinder
    {
        public static async ValueTask SkipToNextToken(ParsingSource source)
        {
            ReadResult seq;
            do
            {
                seq = await source.ReadAsync();
            } while (!SkipToNextToken(source, seq.Buffer));
        }

        private static bool SkipToNextToken(ParsingSource source, ReadOnlySequence<byte> seqBuffer)
        {
            var reader = new SequenceReader<byte>(seqBuffer);
            if (SkipToNextToken(ref reader))
            {
                source.AdvanceTo(reader.Position);
                return true;
            }

            source.NeedMoreInputToAdvance();
            return false;
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
    }
}