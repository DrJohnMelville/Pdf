using System.Threading.Tasks;
using Melville.Pdf.DataModelTests.Standard.S8_4GraphicState;
using Xunit;

namespace Melville.Pdf.DataModelTests.Standard.S9_4Text;

public class TextObjectOperationsParserTest : ParserTest
{
    [Fact]
    public Task BeginTextObject() => TestInput("BT", i => i.BeginTextObject());
    [Fact]
    public Task EndTextObject() => TestInput("ET", i => i.EndTextObject());
    [Fact]
    public Task MovePositionBy() => TestInput("5 6 Td", i => i.MovePositionBy(5,6));
    [Fact]
    public Task MovePositionByWithLeading() => 
        TestInput("5 6 TD", i => i.MovePositionByWithLeading(5,6));
    [Fact]
    public Task SetMatrix() => 
        TestInput("6 5 4 3 2 1 Tm", i => i.SetTextMatrix(6,5,4,3,2,1));
    [Fact]
    public Task MoveToNextLine() => 
        TestInput("T*", i => i.MoveToNextTextLine());
}