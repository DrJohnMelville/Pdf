using System;
using System.Buffers;

namespace Melville.Postscript.Interpreter.Tokenizers;

/// <summary>
/// Classify Characters for postscript and PDF parsing
/// </summary>
public static class CharacterClassifier
{
    /// <summary>
    /// Decide if a character is whitespace
    /// </summary>
    /// <param name="character">Character to check</param>
    /// <returns>True if character is whitespace, false otherwise</returns>
    public static bool IsWhitespace(byte character) =>
        WhiteSpaceChars().IndexOf(character) >= 0;

    /// <summary>
    /// Decide if a character begins a comment
    /// </summary>
    /// <param name="character">Character to check</param>
    /// <returns>True if character is the comment character, false otherwise</returns>
    public static bool IsCommentBeginChar(byte character) => character is (byte)'%';
    
    /// <summary>
    /// Decide if a character might appear in a line end sequence
    /// </summary>
    /// <param name="character">Character to check</param>
    /// <returns>True if character is a line end  character, false otherwise</returns>
    public static bool IsLineEndChar(byte character) =>
        LineEndChars().IndexOf(character) >= 0;

    /// <summary>
    /// All of the characters that end an undelimited name
    /// </summary>
    public static ReadOnlySpan<byte> DelimiterChars() => "\x0\x09\x0A\x0C\x0d ()<>[]{}/%"u8;

    /// <summary>
    /// All the whitespace characters
    /// </summary>
    public static ReadOnlySpan<byte> WhiteSpaceChars() => "\x0\x09\x0A\x0C\x0d "u8;

    /// <summary>
    /// All the characters that might appear in a line end sequence.
    /// </summary>
    /// <returns></returns>
    public static ReadOnlySpan<byte> LineEndChars() => "\r\n"u8;

    /// <summary>
    /// Compute the value for a given digit in a number system up to base 36
    /// </summary>
    /// <param name="digitChar">The digit to convert</param>
    /// <returns>A value for the given character</returns>
    public static byte ValueFromDigit(byte digitChar) => digitChar switch
    {
        >= (byte)'0' and <= (byte)'9' => (byte)(digitChar - '0'),
        >= (byte)'A' and <= (byte)'Z' => (byte)(digitChar - 'A' + 10),
        >= (byte)'a' and <= (byte)'z' => (byte)(digitChar - 'a' + 10),
        _ => byte.MaxValue
    };

    /// <summary>
    /// SearchValues that looks for a hex digit
    /// </summary>
    public static readonly SearchValues<byte> HexDigits = 
        SearchValues.Create("0123456789ABCDEFabcdef"u8);
}