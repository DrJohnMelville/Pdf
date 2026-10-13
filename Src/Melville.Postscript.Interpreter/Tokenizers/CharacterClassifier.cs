using Melville.INPC;
using System;
using System.Buffers;

namespace Melville.Postscript.Interpreter.Tokenizers;

/// <summary>
/// Classify Characters for postscript and PDF parsing
/// </summary>
public static partial class CharacterClassifier
{
    [MacroItem("Whitespace", @"\x0\x09\x0A\x0C\x0d ")]
    [MacroItem("LineEnd", @"\r\n")]
    [MacroItem("Delimiter", @"\x0\x09\x0A\x0C\x0d ()<>[]{}/%")]
    [MacroItem("HexDigit", @"0123456789ABCDEFabcdef")]
    [MacroCode("""
       /// <summary>
       /// Decide if a character is ~0~
       /// </summary>
       /// <param name="character">Character to check</param>
       /// <returns>True if character is ~0~, false otherwise</returns>
       public static bool Is~0~(byte character) =>
           ~0~SearchValues.Contains(character);
       /// <summary>
       /// All the ~0~ characters
       /// </summary>
       public static ReadOnlySpan<byte> ~0~Chars() => "~1~"u8;
   
       /// <summary>
       /// All the ~0~ characters
       /// </summary>
       public static readonly SearchValues<byte> ~0~SearchValues = 
       SearchValues.Create(~0~Chars());
    """)]

    /// <summary>
    /// Decide if a character begins a comment
    /// </summary>
    /// <param name="character">Character to check</param>
    /// <returns>True if character is the comment character, false otherwise</returns>
    public static bool IsCommentBeginChar(byte character) => character is (byte)'%';

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
    /// Compose a byte from the character representation of two hex digits.
    /// </summary>
    /// <param name="char1">The high nibble like '1'</param>
    /// <param name="char2">The low nibble like 'D'</param>

    public static byte ByteFromHexChars(byte char1, byte char2) =>
        (byte)((ValueFromDigit(char1)<<4) | ValueFromDigit(char2));

}