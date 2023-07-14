using System.Buffers;

namespace Melville.Postscript.Interpreter.Tokenizers;

/// <summary>
/// Methods to skip over whitespace and comments
/// </summary>
public static class CommentSkipper
{
    /// <summary>
    /// Skip over comments and whitespace while looking for the first byte of the next token
    /// </summary>
    /// <param name="reader">A sequence reader</param>
    /// <param name="firstChar">The peeked at character</param>
    /// <returns>True if a first character exists, false otherwise.</returns>
    public static bool TryPeekNextNonComment(this ref SequenceReader<byte> reader, out byte firstChar)
    {
        while (true)
        {
            if (!TryPeekNextVisible(ref reader, out firstChar)) return false;
            if (!CharacterClassifier.IsCommentBeginChar(firstChar)) return true;
            if (!TrySkipComment(ref reader)) return false;
        }
    }

    /// <summary>
    /// Advance the reader past any whitespace and then peek at first byte;
    /// </summary>
    /// <param name="reader">A sequence reader</param>
    /// <param name="firstChar">The peeked at character</param>
    /// <returns>True if a first character exists, false otherwise.</returns>
    public static bool TryPeekNextVisible(this ref SequenceReader<byte> reader, out byte firstChar)
    {
        reader.AdvancePastAny(CharacterClassifier.WhiteSpaceChars());
        if (!reader.TryPeek(out firstChar)) return false;
        return true;
    }

    /// <summary>
    /// Try to read the next nonwhitespace character.
    /// </summary>
    /// <param name="reader">A sequence reader</param>
    /// <param name="firstChar">The read character</param>
    /// <returns>True if a first character exists, false otherwise.</returns>
    public static bool TryReadNextVisible(this ref SequenceReader<byte> reader, out byte firstChar)
    {
        reader.AdvancePastAny(CharacterClassifier.WhiteSpaceChars());
        if (!reader.TryRead(out firstChar)) return false;
        return true;
    }

    /// <summary>
    /// Skip to the end of the current line -- which conveniently skips over any comment we are in.
    /// </summary>
    /// <param name="reader"></param>
    /// <returns></returns>
    private static bool TrySkipComment(ref SequenceReader<byte> reader) =>
        reader.TryAdvanceToAny(CharacterClassifier.LineEndChars());
}