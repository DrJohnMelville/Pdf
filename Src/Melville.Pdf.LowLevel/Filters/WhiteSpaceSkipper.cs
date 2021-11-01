using System.Buffers;
using Melville.Pdf.LowLevel.Model.Conventions;

namespace Melville.Pdf.LowLevel.Filters;

public static class WhiteSpaceSkipper
{
    public static bool TryReadNonWhitespace(this ref SequenceReader<byte> source, out byte item)
    {
        while (true)
        {
            if (!source.TryRead(out item)) return false;
            if (CharClassifier.Classify(item) != CharacterClass.White)
                return true;
        }
            
    }
}