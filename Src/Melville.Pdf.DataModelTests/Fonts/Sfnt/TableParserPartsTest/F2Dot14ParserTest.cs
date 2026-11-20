using FluentAssertions;
using Melville.Fonts.SfntParsers.TableParserParts;

namespace Melville.Pdf.DataModelTests.Fonts.Sfnt.TableParserPartsTest;

public class F2Dot14ParserTest()
{
    [Test]
    [Arguments(0X7FFF, 1.999939)]
    [Arguments(0X7000, 1.75)]
    [Arguments(0X0, 0)]
    [Arguments(0XFFFF, -0.000061)]
    [Arguments(0X8000, -2.0)]
    public void Parse(int value, float result)
    {
        F2Dot14Parser.Convert((short)value).Should().BeInRange(
            (float)(result-1E-7), (float)(result+1E-7));
    }
}