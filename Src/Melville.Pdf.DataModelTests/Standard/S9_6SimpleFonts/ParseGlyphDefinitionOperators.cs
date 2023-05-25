using System.Threading.Tasks;
using Melville.Pdf.DataModelTests.Standard.S8_4GraphicState;
using Xunit;

namespace Melville.Pdf.DataModelTests.Standard.S9_6SimpleFonts;

public class ParseGlyphDefinitionOperators : ParserTest
{
    [Fact]
    public Task SetColoredGlyphMetricsAsync() => TestInputAsync(
        "1 2 d0", i => i.SetColoredGlyphMetrics(1, 2));
    [Fact]
    public Task SetUncoloredGlyphMetricsAsync() => TestInputAsync(
        "1 2 3 4 5 6 d1", i => i.SetUncoloredGlyphMetrics(1, 2, 3, 4, 5, 6));
}