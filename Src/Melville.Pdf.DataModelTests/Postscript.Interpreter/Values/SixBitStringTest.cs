using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Postscript.Interpreter.Values;
using Melville.Postscript.Interpreter.Values.Strings;
using Xunit;

namespace Melville.Pdf.DataModelTests.Postscript.Interpreter.Values;

public class SixBitStringTest
{
    private readonly PostscriptShortString sut = StringKind.String.Strategy6Bit;
    [Theory]
    [InlineData("-", 1)]
    [InlineData("0", 2)]
    [InlineData("1", 3)]
    [InlineData("A", 12)]
    [InlineData("B", 13)]
    [InlineData("Z", 37)]
    [InlineData("a", 38)]
    [InlineData("z", 63)]
    [InlineData("--", 0b000001_000001)]
    [InlineData("123", 0b000101_000100_000011)]
    public void EncodeTest(string text, int value)
    {
        Assert.True(sut.TryEncode(text.AsExtendedAsciiBytes(), out var memento));
        Assert.Equal(value, memento.Int128);
        Assert.Equal(text, new PdfDirectObject(sut, new MementoUnion(value)).ToString());
    }

    [Fact]
    public void SixteenByteString()
    {
        Assert.True(StringKind.String.Strategy16Bytes.TryEncode(
            "1234567890123456"u8, out var memento));
        Assert.Equal("1234567890123456",
                     new PdfDirectObject(StringKind.String.Strategy16Bytes, memento).ToString());
    }

    [Theory]
    [InlineData("!")]
    [InlineData("1234567890123456789012")]
    [InlineData("\0")]
    public void ParsingFailure(string value)
    {
        Assert.False(sut.TryEncode(value.AsExtendedAsciiBytes(), out  var _));
    }
}