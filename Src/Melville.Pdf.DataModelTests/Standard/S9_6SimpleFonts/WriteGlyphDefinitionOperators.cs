using System.Threading.Tasks;
using Melville.Pdf.DataModelTests.Standard.S8_4GraphicState;
using Xunit;

namespace Melville.Pdf.DataModelTests.Standard.S9_6SimpleFonts;

public class WriteGlyphDefinitionOperators: WriterTest
{
    [Fact] 
    public async Task SetColoredGlyphMetric()
    {
        sut.SetColoredGlyphMetrics(1, 2);
        Assert.Equal("1 2 d0\n", await WrittenText());
    }
    
    [Fact] 
    public async Task SetUncoloredGlyphMetric()
    {
        sut.SetUncoloredGlyphMetrics(1, 2, 3, 4, 5, 6);
        Assert.Equal("1 2 3 4 5 6 d1\n", await WrittenText());
    }

}