using System.Buffers;

namespace Melville.Postscript.Interpreter.Tokenizers;

internal static class CommentSkipper
{
    public static bool TryPeedNextNonComment(this ref SequenceReader<byte> reader, out byte firstChar)
    {
        while (true)
        {
            if (!TryPeekNextVisible(ref reader, out firstChar)) return false;
            if (!CharacterClassifier.IsCommentBeginChar(firstChar)) return true;
            if (!TrySkipComment(ref reader)) return false;
        }
    }

    public static bool TryPeekNextVisible(this ref SequenceReader<byte> reader, out byte firstChar)
    {
        reader.AdvancePastAny(CharacterClassifier.WhiteSpaceChars());
        if (!reader.TryPeek(out firstChar)) return false;
        return true;
    }

    public static bool TryReadNextVisible(this ref SequenceReader<byte> reader, out byte firstChar)
    {
        reader.AdvancePastAny(CharacterClassifier.WhiteSpaceChars());
        if (!reader.TryRead(out firstChar)) return false;
        return true;
    }

    private static bool TrySkipComment(ref SequenceReader<byte> reader) =>
        reader.TryAdvanceToAny(CharacterClassifier.LineEndChars());
}