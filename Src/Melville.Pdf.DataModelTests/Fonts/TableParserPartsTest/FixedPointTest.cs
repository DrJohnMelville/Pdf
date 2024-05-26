using FluentAssertions;
using Melville.Fonts.SfntParsers.TableParserParts;
using Xunit;

namespace Melville.Pdf.DataModelTests.Fonts.TableParserPartsTest;

public class FixedPointTest
{
    [Theory]
    [InlineData(0x00010000, "1")]
    [InlineData(0x00018000, "1.5")]
    [InlineData(0x00014000, "1.25")]
    [InlineData(0x00010002, "1.00003051757812")]
    public void FixedPointPrinterTest(int value, string expected)
    {
        new FixedPoint<int, long, Fixed16>(value).ToString().Should().Be(expected);
        ("-" + expected).Should().StartWith(new FixedPoint<int, long, Fixed16>(-value).ToString());
    }
}