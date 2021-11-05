using System.Threading.Tasks;
using Melville.Pdf.DataModelTests.Standard.S8_4GraphicState;
using Melville.Pdf.LowLevel.Model.ContentStreams;
using Xunit;

namespace Melville.Pdf.DataModelTests.Standard.S14_6MarkedContent;

public class MarkedContentWriterTest: WriterTest
{
    [Fact]
    public async Task MarkedContentPointTest()
    {
        sut.MarkedContentPoint("M1");
        Assert.Equal("/M1 MP\n", await WrittenText() );
        
    }

    [Fact]
    public async Task MarkedPointWithPropertyName()
    {
        sut.MarkedContentPoint("M1", "M2");
        Assert.Equal("/M1 /M2 DP\n", await WrittenText());
        
    }
    // [Fact]
    // public async Task MarkedPointWithInlineDictionary()
    // {
    //     sut.MarkedContentPoint("M1", "M2");
    //     Assert.Equal("/M1 /M2 DP\n", await WrittenText());
    //     
    // }
 
}