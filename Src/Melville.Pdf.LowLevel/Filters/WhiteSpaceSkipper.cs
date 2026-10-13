using System.Buffers;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Postscript.Interpreter.Tokenizers;

namespace Melville.Pdf.LowLevel.Filters;

internal static class WhiteSpaceSkipper
{
    public static bool TryReadNonWhitespace(this ref SequenceReader<byte> source, out byte item)
    {
        while (true)
        {
            if (!source.TryRead(out item)) return false;
            if (!CharacterClassifier.IsWhitespace(item))
                return true;
        }
            
    }
}