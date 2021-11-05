using System.Threading.Tasks;
using Melville.Pdf.DataModelTests.Standard.S8_4GraphicState;
using Melville.Pdf.LowLevel.Model.Conventions;
using Xunit;

namespace Melville.Pdf.DataModelTests.Standard.S14_6MarkedContent;

public class MarkedContentParser : ParserTest
{
    [Fact]
    public Task MarkedContentPoint() =>
        TestInput("/M1 MP", i => i.MarkedContentPoint(NameDirectory.Get("M1")));
    [Fact]
    public Task MarkedContentPointWithNamedParam() =>
        TestInput("/M1 /M2 DP", i => i.MarkedContentPoint(
            "M1", "M2"));
}