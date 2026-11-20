using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Postscript.Interpreter.Values;
using Melville.Postscript.Interpreter.Values.Strings;

namespace Melville.Pdf.DataModelTests.Postscript.Interpreter.Values;

public class SixBitStringTest
{
    private readonly PostscriptShortString sut = StringKind.String.Strategy6Bit;
    [Test]
    [Arguments("-", 1)]
    [Arguments("0", 2)]
    [Arguments("1", 3)]
    [Arguments("A", 12)]
    [Arguments("B", 13)]
    [Arguments("Z", 37)]
    [Arguments("a", 38)]
    [Arguments("z", 63)]
    [Arguments("--", 0b000001_000001)]
    [Arguments("123", 0b000101_000100_000011)]
    public void EncodeTest(string text, int value)
    {
        Assert.True(sut.TryEncode(text.AsExtendedAsciiBytes(), out var memento));
        Assert.Equal(value, memento.Int128);
        Assert.Equal(text, new PdfDirectObject(sut, new MementoUnion(value)).ToString());
    }

    [Test]
    public void SixteenByteString()
    {
        Assert.True(StringKind.String.Strategy16Bytes.TryEncode(
            "1234567890123456"u8, out var memento));
        Assert.Equal("1234567890123456",
                     new PdfDirectObject(StringKind.String.Strategy16Bytes, memento).ToString());
    }

    [Test]
    [Arguments("!")]
    [Arguments("1234567890123456789012")]
    [Arguments("\0")]
    public void ParsingFailure(string value)
    {
        Assert.False(sut.TryEncode(value.AsExtendedAsciiBytes(), out  var _));
    }
}