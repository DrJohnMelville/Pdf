using System;
using System.Diagnostics;

namespace Melville.Postscript.Interpreter.Tokenizers
{
    internal class CommentSkipTokenizer : WhiteSpaceSkipper
    {
        // Essentially a comment is a special kind of whitespace that egins with the remainder of the line
        // plus any whitespace after that.  So we lop off the first line and then use the base method to skip
        // terminal whitespace.
        protected override bool TryFindLength(ReadOnlySpan<byte> source, out int length)
        {
            Debug.Assert(CharacterClassifier.IsCommentBeginChar(source[0]));
            length = source.IndexOfAny(CharacterClassifier.LineEndSearchValues);
            if (length < 0) return false;
            base.TryFindLength(source[length..], out var wsLength);
            length += wsLength;
            return true;
        }
    }
}
