using System.IO;
using System.IO.Pipelines;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.ContentStreams;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Parsing.ContentStreams;
using Moq;
using Xunit;

namespace Melville.Pdf.DataModelTests.Standard.S8_4GraphicState;

public class GraphicStateParsers
{
    private readonly Mock<IContentStreamOperations> target = new();
    private readonly ContentStreamParser sut;

    public GraphicStateParsers()
    {
        sut = new ContentStreamParser(target.Object);
    }

    private ValueTask ParseString(string s) => sut.Parse(PipeReaderFromString(s));
    private static PipeReader PipeReaderFromString(string s) => 
        PipeReader.Create(new MemoryStream(s.AsExtendedAsciiBytes()));

    [Fact]
    public async Task PushGraphicsState()
    {
        await ParseString("q");
        target.Verify(i=>i.SaveGraphicsState());
        target.VerifyNoOtherCalls();
    }
    
    [Fact]
    public async Task PopGraphicsState()
    {
        await ParseString("Q");
        target.Verify(i=>i.RestoreGraphicsState());
        target.VerifyNoOtherCalls();
    }    
    [Fact]
    public async Task CompositeOperatorsWithWhiteSpace()
    {
        await ParseString("   q\r\n  Q  ");
        target.Verify(i=>i.SaveGraphicsState());
        target.Verify(i=>i.RestoreGraphicsState());
        target.VerifyNoOtherCalls();
    }
    [Fact]
    public async Task ModifyTransformMatrixTest()
    {
        await ParseString("1.000 2 03 +4 5.5 -6 cm");
        target.Verify(i=>i.ModifyTransformMatrix(1, 2, 3, 4, 5.5, -6));
        target.VerifyNoOtherCalls();
    }
    [Fact]
    public async Task SetLineWidthTest()
    {
        await ParseString("256 w");
        target.Verify(i=>i.SetLineWidth(256));
        target.VerifyNoOtherCalls();
    }
}