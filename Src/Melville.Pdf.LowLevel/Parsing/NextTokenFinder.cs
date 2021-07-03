using System.Buffers;

namespace Melville.Pdf.LowLevel.Parsing
{
    public static class NextTokenFinder
    {
        public static bool SkipToNextToken(ref SequenceReader<byte> input)
        {
            while (true)
            {
                if (!input.TryRead(out var character)) return false;
                switch (CharClassifier.Classify(character))
                {
                    case CharacterClass.White:
                        break;
                    case CharacterClass.Delimiter when character==(byte)'%':
                        if (!input.TrySkipToEndOfLineMarker()) return false;
                        break;
                    default:
                        input.Rewind(1);
                        return true;
                }
            }
        }
    }
}