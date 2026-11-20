using System;
using Melville.Pdf.LowLevel.Encryption.StringFilters;

namespace Melville.Pdf.DataModelTests.Standard.S7_6Encryption.S7_6_3_4PasswordAlgorithms.V6Algorithms;

public class SamlPrepFilterTest
{
    [Test]
    [Arguments("user", "user")]
    [Arguments("USER", "USER")]
    [Arguments("I\xADX", "IX")]
    [Arguments("\x00AA", "a")]
    [Arguments("\x2168", "IX")]
    public void StringsFromRfc4013(string source, string destination)
    {
        Assert.Equal(destination, source.SaslPrep());
    }

    [Test]
    [Arguments('\x00A0')]
    [Arguments('\x1680')]
    [Arguments('\x2000')]
    [Arguments('\x2001')]
    [Arguments('\x2002')]
    [Arguments('\x2003')]
    [Arguments('\x2004')]
    [Arguments('\x2005')]
    [Arguments('\x2006')]
    [Arguments('\x2007')]
    [Arguments('\x2008')]
    [Arguments('\x2009')]
    [Arguments('\x200A')]
    [Arguments('\x202F')]
    [Arguments('\x205F')]
    [Arguments('\x3000')]
    public void MapsToSpace(char item)
    {
        Assert.Equal("[ ]", $"[{item}]".SaslPrep());
    }

    [Test]
    [Arguments('\x00AD')]
    [Arguments('\x034F')]
    [Arguments('\x1806')]
    [Arguments('\x180B')]
    [Arguments('\x180C')]
    [Arguments('\x180D')]
    [Arguments('\x200B')]
    [Arguments('\x200C')]
    [Arguments('\x200D')]
    [Arguments('\x2060')]
    [Arguments('\xFE00')]
    [Arguments('\xFE01')]
    [Arguments('\xFE02')]
    [Arguments('\xFE03')]
    [Arguments('\xFE04')]
    [Arguments('\xFE05')]
    [Arguments('\xFE06')]
    [Arguments('\xFE07')]
    [Arguments('\xFE08')]
    [Arguments('\xFE09')]
    [Arguments('\xFE0A')]
    [Arguments('\xFE0B')]
    [Arguments('\xFE0C')]
    [Arguments('\xFE0D')]
    [Arguments('\xFE0E')]
    [Arguments('\xFE0F')]
    [Arguments('\xFEFF')]
    public void MapsToNothing(char item)
    {
        Assert.Equal("[]", $"[{item}]".SaslPrep());
    }

    [Test]
    [Arguments("\x001D")]
    [Arguments("\x0627\x0031")]
    [Arguments("\x0627a\x0031\x0628")]
    public void InvalidSasl(string input) =>
        input.Invoking(i => i.SaslPrepIfValid()).Should().Throw<ArgumentException>();

    [Test]
    [Arguments("\x0020")]
    [Arguments("\x0627\x0031\x0628")]
    public void ValidSasl(string input) =>
        input.SaslPrepIfValid(); // implicitly, does not throw

     [Test]
    [Arguments('\x0000', '\x001f', CharacterKind.Error)]
    [Arguments('\x0020', '\x007E', CharacterKind.None)]
    [Arguments('\x007f', '\x007f', CharacterKind.Error)]
    [Arguments('\x0080', '\x009F', CharacterKind.Error)]
    [Arguments('\x06DD', '\x06DD', CharacterKind.Error)]
    [Arguments('\x070F', '\x070F', CharacterKind.Error)]
    [Arguments('\x180E', '\x180E', CharacterKind.Error)]
    [Arguments('\x200C', '\x200C', CharacterKind.Error)]
    [Arguments('\x200D', '\x200D', CharacterKind.Error)]
    [Arguments('\x2028', '\x2028', CharacterKind.Error)]
    [Arguments('\x2029', '\x2029', CharacterKind.Error)]
    [Arguments('\x2060', '\x2060', CharacterKind.Error)]
    [Arguments('\x2061', '\x2061', CharacterKind.Error)]
    [Arguments('\x2062', '\x2062', CharacterKind.Error)]
    [Arguments('\x2063', '\x2063', CharacterKind.Error)]
    [Arguments('\x206A', '\x206F', CharacterKind.Error)]
    [Arguments('\xFEFF', '\xFEFF', CharacterKind.Error)]
    [Arguments('\xFFF9', '\xFFFC', CharacterKind.Error)]
    [Arguments('\xE000', '\xF8FF', CharacterKind.Error)]
    [Arguments('\xFDD0', '\xFDEF', CharacterKind.Error)]
    [Arguments('\xFFFE', '\xFFFF', CharacterKind.Error)]
    [Arguments('\xD800', '\xDFFF', CharacterKind.Error)]
    [Arguments('\xFFF9', '\xFFF9', CharacterKind.Error)]
    [Arguments('\xFFFA', '\xFFFA', CharacterKind.Error)]
    [Arguments('\xFFFB', '\xFFFB', CharacterKind.Error)]
    [Arguments('\xFFFC', '\xFFFC', CharacterKind.Error)]
    [Arguments('\xFFFD', '\xFFFD', CharacterKind.Error)]
    [Arguments('\x2FF0', '\x2FFB', CharacterKind.Error)]
    [Arguments('\x0340', '\x0340', CharacterKind.Error)]
    [Arguments('\x0341', '\x0341', CharacterKind.Error)]
    [Arguments('\x200E', '\x200E', CharacterKind.Error)]
    [Arguments('\x200F', '\x200F', CharacterKind.Error)]
    [Arguments('\x202A', '\x202A', CharacterKind.Error)]
    [Arguments('\x202B', '\x202B', CharacterKind.Error)]
    [Arguments('\x202C', '\x202C', CharacterKind.Error)]
    [Arguments('\x202D', '\x202D', CharacterKind.Error)]
    [Arguments('\x202E', '\x202E', CharacterKind.Error)]
    [Arguments('\x206A', '\x206A', CharacterKind.Error)]
    [Arguments('\x206B', '\x206B', CharacterKind.Error)]
    [Arguments('\x206C', '\x206C', CharacterKind.Error)]
    [Arguments('\x206D', '\x206D', CharacterKind.Error)]
    [Arguments('\x206E', '\x206E', CharacterKind.Error)]
    [Arguments('\x206F', '\x206F', CharacterKind.Error)]
    
    internal void GetCharacterKindTest(char low, char high, CharacterKind kind)
    {
        // we need to check high and low on each iteration due to overflowing the top of char
        for (char i = low; i <= high && i >= low; i++)
        {
            Assert.Equal(kind != CharacterKind.Error, SaslValidator.IsValid($"{i}"));
                        
        }
    }
}