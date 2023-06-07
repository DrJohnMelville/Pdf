using System;

namespace Melville.Postscript.Interpreter.Tokenizers
{
    internal static class CharacterClassifier
    {
        public static bool IsWhitespace(byte character) =>
            character is 0 or 9 or 10 or 12 or 13 or 32;

        public static bool IsCommentBeginChar(byte b) => b is (byte)'%';

        public static ReadOnlySpan<byte> DelimiterChars() => "\x0\x09\x0A\x0Cx\x0d ()<>[]{}/%"u8;
        public static ReadOnlySpan<byte> WhiteSpaceChars() => "\x0\x09\x0A\x0Cx\x0d "u8;
        public static ReadOnlySpan<byte> LineEndChars() => "\x0C\x0D"u8;
    }
}