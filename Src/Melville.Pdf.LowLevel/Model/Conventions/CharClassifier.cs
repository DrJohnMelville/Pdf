using System;

namespace Melville.Pdf.LowLevel.Model.Conventions;

/// <summary>
/// PDF specifies three classes of characters, whitespace, delimiters, and regular.  This represents that concept
/// </summary>
public enum CharacterClass
{
    /// <summary>
    /// Neither a white space nor a delimiter
    /// </summary>
    Regular = 0,
    /// <summary>
    /// Whitespace character -- one of \x00, \x09, \x0A, \x0C, \x0D, \x20
    /// </summary>
    White = 1,
    /// <summary>
    /// One of (, ), &lt;, &gt;, [, ], {. }, /, or %
    /// </summary>
    Delimiter = 2
}

/// <summary>
/// PDF specifies three classes of characters, whitespace, delimiters, and regular.  This classifies all characters into exactly one
/// of these classes.
/// </summary>
public static class CharClassifier
{
    /// <summary>
    /// Classifies a byte into one of 3 character classes.
    /// </summary>
    /// <param name="input">The byte to classify</param>
    /// <returns>The class that byte beloings to.</returns>
    public static CharacterClass Classify(byte input) =>
        input switch
        {
            var x when IsWhite(x) => CharacterClass.White,
            var x when IsDelimiter(x)=> CharacterClass.Delimiter,
            _ => CharacterClass.Regular
        };

    /// <summary>
    /// Check if the given character is a whitespace character
    /// </summary>
    /// <param name="input">Character to check</param>
    /// <returns>True if the character is whitespace, false otherwise.</returns>
    public static bool IsWhite(byte input) => "\x00\x09\x0A\x0C\x0D\x20"u8.Contains(input);
    /// <summary>
    /// Check if the given character is a delimiter character
    /// </summary>
    /// <param name="input">Character to check</param>
    /// <returns>True if the character is a delimiter character, false otherwise.</returns>
    public static bool IsDelimiter(byte input) => "()<>[]{}/%"u8.Contains(input);
    /// <summary>
    /// Check if the given character is a regular character
    /// </summary>
    /// <param name="input">Character to check</param>
    /// <returns>True if the character is a regular character, false otherwise.</returns>
    public static bool IsRegular(byte input) => !(IsWhite(input) || IsDelimiter(input));
}