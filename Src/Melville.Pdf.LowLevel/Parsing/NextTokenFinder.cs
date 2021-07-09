using System.Buffers;
using System.IO.Pipelines;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Parsing.PdfStreamHolders;

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