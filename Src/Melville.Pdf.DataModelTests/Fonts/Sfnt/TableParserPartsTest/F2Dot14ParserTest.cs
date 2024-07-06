using FluentAssertions;
using Melville.Fonts.SfntParsers.TableParserParts;
using Xunit;

namespace Melville.Pdf.DataModelTests.Fonts.Sfnt.TableParserPartsTest;

public class F2Dot14ParserTest()
{
    [Theory]
    [InlineData(0X7FFF, 1.999939)]
    [InlineData(0X7000, 1.75)]
    [InlineData(0X0, 0)]
    [InlineData(0XFFFF, -0.000061)]
    [InlineData(0X8000, -2.0)]
    public void Parse(int value, float result)
    {
        F2Dot14Parser.Convert((short)value).Should().BeInRange(
            (float)(result-1E-7), (float)(result+1E-7));
    }
}