using System.Buffers;

namespace Melville.Postscript.Interpreter.Tokenizers;

internal static class CommentSkipper
{
    public static bool SkipWhiteSpace(ref SequenceReader<byte> reader, out byte firstChar)
    {
        while (true)
        {
            reader.AdvancePastAny(CharacterClassifier.WhiteSpaceChars());
            if (!reader.TryPeek(out firstChar)) return false;
            if (!CharacterClassifier.IsCommentBeginChar(firstChar)) return true;
            if (!TrySkipComment(ref reader)) return false;
        }
    } 
    
    private static bool TrySkipComment(ref SequenceReader<byte> reader) =>
        reader.TryAdvanceToAny(CharacterClassifier.LineEndChars());
}