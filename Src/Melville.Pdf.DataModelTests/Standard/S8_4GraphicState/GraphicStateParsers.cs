using System;
using System.Threading.Tasks;
using Melville.INPC;
using Melville.Pdf.LowLevel.Model.ContentStreams;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Primitives;
using Xunit;

namespace Melville.Pdf.DataModelTests.Standard.S8_4GraphicState;

public partial class GraphicStateParsers: ParserTest
{
    [Fact]
    public Task PushGraphicsState() => TestInput("q", i => i.SaveGraphicsState());

    [Fact]
    public Task PopGraphicsState() => TestInput(
        "Q", i => i.RestoreGraphicsState());

    [Fact]
    public async Task CompositeOperatorsWithWhiteSpace()
    {
        await ParseString("   q\r\n  Q  ");
        Target.Verify(i => i.SaveGraphicsState());
        Target.Verify(i => i.RestoreGraphicsState());
        Target.VerifyNoOtherCalls();
    }

    [Fact]
    public Task ModifyTransformMatrixTest() => TestInput(
        "1.000 2 03 +4 5.5 -6 cm", i => i.ModifyTransformMatrix(1, 2, 3, 4, 5.5, -6));

    [Fact]
    public Task SetLineWidthTest() => TestInput("256 w", i => i.SetLineWidth(256));

    [Fact]
    public Task SetMiterLimit() => 
        TestInput("10 M", i => i.SetMiterLimit(10));

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

    private partial class DashArrayMock:MockBase, IContentStreamOperations
    {
        [DelegateTo] private IContentStreamOperations fake = null!;
        private readonly double phase;
        private readonly double[] dots;

        public DashArrayMock(double phase, double[] dots)
        {
            this.phase = phase;
            this.dots = dots;
        }

        public void SetLineDashPattern(double delta, in ReadOnlySpan<double> dashes)
        {
            Assert.Equal(phase, delta);
            Assert.Equal(dots, dashes.ToArray());
            SetCalled();
            
        }
    }
    
    [Fact]
    public Task SetDashPattern2() =>
        TestInput("[1 2] 3 d",
            new DashArrayMock(3, new double[] { 1, 2 }));
    
    [Fact]
    public Task SetDashPattern1() =>
        TestInput("[1] 3 d",
            new DashArrayMock(3, new double[] { 1 }));
    
    [Fact]
    public Task SetDashPattern0() =>
        TestInput("[] 3 d", new DashArrayMock(3, new double[0]));

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