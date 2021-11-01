using System;
using System.IO;
using System.IO.Pipelines;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.ContentStreams;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.LowLevel.Parsing.ContentStreams;
using Moq;
using Xunit;

namespace Melville.Pdf.DataModelTests.Standard.S8_4GraphicState;

public class GraphicStateParsers
{
    private readonly Mock<ConcreteCSO> target = new();
    private readonly ContentStreamParser sut;

    public GraphicStateParsers()
    {
        sut = new ContentStreamParser(target.Object);
    }

    private ValueTask ParseString(string s) => sut.Parse(PipeReaderFromString(s));

    private static PipeReader PipeReaderFromString(string s) =>
        PipeReader.Create(new MemoryStream(s.AsExtendedAsciiBytes()));

    private async Task TestInput(
        string input, Expression<Action<ConcreteCSO>> action)
    {
        await ParseString(input);
        target.Verify(action);
        target.VerifyNoOtherCalls();
    }

    [Fact]
    public Task PushGraphicsState() => TestInput("q", i => i.SaveGraphicsState());

    [Fact]
    public Task PopGraphicsState() => TestInput(
        "Q", i => i.RestoreGraphicsState());

    [Fact]
    public async Task CompositeOperatorsWithWhiteSpace()
    {
        await ParseString("   q\r\n  Q  ");
        target.Verify(i => i.SaveGraphicsState());
        target.Verify(i => i.RestoreGraphicsState());
        target.VerifyNoOtherCalls();
    }

    [Fact]
    public Task ModifyTransformMatrixTest() => TestInput(
        "1.000 2 03 +4 5.5 -6 cm", i => i.ModifyTransformMatrix(1, 2, 3, 4, 5.5, -6));

    [Fact]
    public Task SetLineWidthTest() => TestInput("256 w", i => i.SetLineWidth(256));

    [Theory]
    [InlineData(LineCap.Butt)]
    [InlineData(LineCap.Round)]
    [InlineData(LineCap.Square)]
    public Task SetLineCap(LineCap cap) => TestInput(
        $"{(int)cap} J", i => i.SetLineCap(cap));

    [Theory]
    [InlineData(LineJoinStyle.Miter, 0)]
    [InlineData(LineJoinStyle.Round, 1)]
    [InlineData(LineJoinStyle.Bevel, 2)]
    public Task SetLineJoinStyle(LineJoinStyle joinStyle, int num) =>
        TestInput($"{num} j", i => i.SetLineJoinStyle(joinStyle));

    [Fact]
    public Task SetDashPattern2() =>
        TestInput("[1 2] 3 d",
            i => i.TestSetLineDashPattern(3, new double[] { 1, 2 }));

    [Fact]
    public Task SetDashPattern1() =>
        TestInput("[1] 3 d",
            i => i.TestSetLineDashPattern(3, new double[] { 1 }));

    [Fact]
    public Task SetDashPattern0() =>
        TestInput("[] 3 d",
            i => i.TestSetLineDashPattern(3, new double[0]));

    [Fact]
    public Task ParseRenderingIntent() =>
        TestInput("/Perceptual ri", i => i.SetRenderIntent(RenderingIntentName.Perceptual));

    [Fact]
    public Task ParseRenderingIntentFail() =>
        Assert.ThrowsAsync<PdfParseException>(() =>
            TestInput("/Pages ri", i => i.SetRenderIntent(RenderingIntentName.Perceptual)));
    [Fact]
    public Task ParseFlatnessTolerance() =>
            TestInput("27 i", i => i.SetFlatnessTolerance(27));
    [Fact]
    public Task LoadGsDictionary()
    {
        var name = NameDirectory.Get("JdmState");
        return TestInput("/JdmState gs", i => i.LoadGraphicStateDictionary(name));
    }
}