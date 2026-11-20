using System.Numerics;
using Melville.Pdf.TextExtractor;

namespace Melville.Pdf.DataModelTests.TextExtractor;

public class TextTargetTest
{
    private readonly ConcatenatingTextTarget sut = new();

    [Test]
    public void WriteSingleCharacter()
    {
        sut.WriteCharacter('Z', default);
        Assert.Equal("Z", sut.AllText());
    }
    [Test]
    public void WriteTwoCharacters()
    {
        sut.WriteCharacter('Z', default);
        sut.WriteCharacter('X', default);
        Assert.Equal("ZX", sut.AllText());
    }

    [Test]
    public void WriteTwoDifferentLocations()
    {
        sut.WriteCharacter('Z', default);
        sut.EndWrite(Matrix3x2.CreateTranslation(22,0));
        sut.WriteCharacter('X', Matrix3x2.CreateTranslation(0, 15));
        Assert.Equal("Z\r\nX", sut.AllText());
    }
    [Test]
    public void WriteTwoContiguiousLocations()
    {
        sut.WriteCharacter('Z', default);
        sut.EndWrite(Matrix3x2.CreateTranslation(22,0));
        sut.WriteCharacter('X', Matrix3x2.CreateTranslation(22.25f, -.12f));
        Assert.Equal("ZX", sut.AllText());
    }
}