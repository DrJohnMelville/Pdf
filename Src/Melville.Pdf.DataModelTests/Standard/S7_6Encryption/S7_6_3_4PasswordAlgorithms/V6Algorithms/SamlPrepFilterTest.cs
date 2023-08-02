using System;
using Melville.Pdf.LowLevel.Encryption.StringFilters;
using Xunit;

namespace Melville.Pdf.DataModelTests.Standard.S7_6Encryption.S7_6_3_4PasswordAlgorithms.V6Algorithms;

public class SamlPrepFilterTest
{
    [Theory]
    [InlineData("user", "user")]
    [InlineData("USER", "USER")]
    [InlineData("I\xADX", "IX")]
    [InlineData("\x00AA", "a")]
    [InlineData("\x2168", "IX")]
    public void StringsFromRfc4013(string source, string destination)
    {
        Assert.Equal(destination, source.SaslPrep());
    }

    [Theory]
    [InlineData('\x00A0')]
    [InlineData('\x1680')]
    [InlineData('\x2000')]
    [InlineData('\x2001')]
    [InlineData('\x2002')]
    [InlineData('\x2003')]
    [InlineData('\x2004')]
    [InlineData('\x2005')]
    [InlineData('\x2006')]
    [InlineData('\x2007')]
    [InlineData('\x2008')]
    [InlineData('\x2009')]
    [InlineData('\x200A')]
    [InlineData('\x202F')]
    [InlineData('\x205F')]
    [InlineData('\x3000')]
    public void MapsToSpace(char item)
    {
        Assert.Equal("[ ]", $"[{item}]".SaslPrep());
    }

    [Theory]
    [InlineData('\x00AD')]
    [InlineData('\x034F')]
    [InlineData('\x1806')]
    [InlineData('\x180B')]
    [InlineData('\x180C')]
    [InlineData('\x180D')]
    [InlineData('\x200B')]
    [InlineData('\x200C')]
    [InlineData('\x200D')]
    [InlineData('\x2060')]
    [InlineData('\xFE00')]
    [InlineData('\xFE01')]
    [InlineData('\xFE02')]
    [InlineData('\xFE03')]
    [InlineData('\xFE04')]
    [InlineData('\xFE05')]
    [InlineData('\xFE06')]
    [InlineData('\xFE07')]
    [InlineData('\xFE08')]
    [InlineData('\xFE09')]
    [InlineData('\xFE0A')]
    [InlineData('\xFE0B')]
    [InlineData('\xFE0C')]
    [InlineData('\xFE0D')]
    [InlineData('\xFE0E')]
    [InlineData('\xFE0F')]
    [InlineData('\xFEFF')]
    public void MapsToNothing(char item)
    {
        Assert.Equal("[]", $"[{item}]".SaslPrep());
    }

    [Theory]
    [InlineData("\x001D")]
    [InlineData("\x0627\x0031")]
    [InlineData("\x0627a\x0031\x0628")]
    public void InvalidSasl(string input) =>
        Assert.Throws<ArgumentException>(input.SaslPrepIfValid);

    [Theory]
    [InlineData("\x0020")]
    [InlineData("\x0627\x0031\x0628")]
    public void ValidSasl(string input) =>
        input.SaslPrepIfValid(); // implicitly, does not throw

    [Theory]
    [InlineData('\x0000', '\x001f', CharacterKind.Error)]
    [InlineData('\x0020', '\x007E', CharacterKind.None)]
    [InlineData('\x007f', '\x007f', CharacterKind.Error)]
    [InlineData('\x0080', '\x009F', CharacterKind.Error)]
    [InlineData('\x06DD', '\x06DD', CharacterKind.Error)]
    [InlineData('\x070F', '\x070F', CharacterKind.Error)]
    [InlineData('\x180E', '\x180E', CharacterKind.Error)]
    [InlineData('\x200C', '\x200C', CharacterKind.Error)]
    [InlineData('\x200D', '\x200D', CharacterKind.Error)]
    [InlineData('\x2028', '\x2028', CharacterKind.Error)]
    [InlineData('\x2029', '\x2029', CharacterKind.Error)]
    [InlineData('\x2060', '\x2060', CharacterKind.Error)]
    [InlineData('\x2061', '\x2061', CharacterKind.Error)]
    [InlineData('\x2062', '\x2062', CharacterKind.Error)]
    [InlineData('\x2063', '\x2063', CharacterKind.Error)]
    [InlineData('\x206A', '\x206F', CharacterKind.Error)]
    [InlineData('\xFEFF', '\xFEFF', CharacterKind.Error)]
    [InlineData('\xFFF9', '\xFFFC', CharacterKind.Error)]
    [InlineData('\xE000', '\xF8FF', CharacterKind.Error)]
    [InlineData('\xFDD0', '\xFDEF', CharacterKind.Error)]
    [InlineData('\xFFFE', '\xFFFF', CharacterKind.Error)]
    [InlineData('\xD800', '\xDFFF', CharacterKind.Error)]
    [InlineData('\xFFF9', '\xFFF9', CharacterKind.Error)]
    [InlineData('\xFFFA', '\xFFFA', CharacterKind.Error)]
    [InlineData('\xFFFB', '\xFFFB', CharacterKind.Error)]
    [InlineData('\xFFFC', '\xFFFC', CharacterKind.Error)]
    [InlineData('\xFFFD', '\xFFFD', CharacterKind.Error)]
    [InlineData('\x2FF0', '\x2FFB', CharacterKind.Error)]
    [InlineData('\x0340', '\x0340', CharacterKind.Error)]
    [InlineData('\x0341', '\x0341', CharacterKind.Error)]
    [InlineData('\x200E', '\x200E', CharacterKind.Error)]
    [InlineData('\x200F', '\x200F', CharacterKind.Error)]
    [InlineData('\x202A', '\x202A', CharacterKind.Error)]
    [InlineData('\x202B', '\x202B', CharacterKind.Error)]
    [InlineData('\x202C', '\x202C', CharacterKind.Error)]
    [InlineData('\x202D', '\x202D', CharacterKind.Error)]
    [InlineData('\x202E', '\x202E', CharacterKind.Error)]
    [InlineData('\x206A', '\x206A', CharacterKind.Error)]
    [InlineData('\x206B', '\x206B', CharacterKind.Error)]
    [InlineData('\x206C', '\x206C', CharacterKind.Error)]
    [InlineData('\x206D', '\x206D', CharacterKind.Error)]
    [InlineData('\x206E', '\x206E', CharacterKind.Error)]
    [InlineData('\x206F', '\x206F', CharacterKind.Error)]
    
    internal void GetCharacterKindTest(char low, char high, CharacterKind kind)
    {
        // we need to check high and low on each iteration due to overflowing the top of char
        for (char i = low; i <= high && i >= low; i++)
        {
            Assert.Equal(kind != CharacterKind.Error, SaslValidator.IsValid($"{i}"));
                        
        }
    }
}