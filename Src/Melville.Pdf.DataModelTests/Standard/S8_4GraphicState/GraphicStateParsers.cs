using System;
using System.Numerics;
using System.Threading.Tasks;
using Melville.INPC;
using Melville.Pdf.LowLevel.Model.ContentStreams;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Objects2;
using Melville.Pdf.LowLevel.Model.Primitives;
using Xunit;

namespace Melville.Pdf.DataModelTests.Standard.S8_4GraphicState;

public partial class GraphicStateParsers: ParserTest
{
    [Fact]
    public Task PushGraphicsStateAsync() => TestInputAsync("q", i => i.SaveGraphicsState());

    [Fact]
    public Task PopGraphicsStateAsync() => TestInputAsync(
        "Q", i => i.RestoreGraphicsState());

    [Fact]
    public Task IgnoreCommendTestAsync() => TestInputAsync(
        "q% this is a q Q q comment \r\n Q", i => i.SaveGraphicsState(), i => i.RestoreGraphicsState());
    
    [Fact]
    public async Task CompositeOperatorsWithWhiteSpaceAsync()
    {
        await ParseStringAsync("   q\r\n  Q  ");
        Target.Verify(i => i.SaveGraphicsState());
        Target.Verify(i => i.RestoreGraphicsState());
        Target.VerifyNoOtherCalls();
    }

    [Fact]
    public Task ModifyTransformMatrixTestAsync() => TestInputAsync(
        "1.000 2 03 +4 5.5 -6 cm", i => i.ModifyTransformMatrix(new Matrix3x2(1, 2, 3, 4, 5.5f, -6)));

    [Fact]
    public Task SetLineWidthTestAsync() => TestInputAsync("256 w", i => i.SetLineWidth(256));

    [Fact]
    public Task SetMiterLimitAsync() => 
        TestInputAsync("10 M", i => i.SetMiterLimit(10));

    [Theory]
    [InlineData(LineCap.Butt)]
    [InlineData(LineCap.Round)]
    [InlineData(LineCap.Square)]
    public Task SetLineCapAsync(LineCap cap) => TestInputAsync(
        $"{(int)cap} J", i => i.SetLineCap(cap));

    [Theory]
    [InlineData(LineJoinStyle.Miter, 0)]
    [InlineData(LineJoinStyle.Round, 1)]
    [InlineData(LineJoinStyle.Bevel, 2)]
    public Task SetLineJoinStyleAsync(LineJoinStyle joinStyle, int num) =>
        TestInputAsync($"{num} j", i => i.SetLineJoinStyle(joinStyle));

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
    public Task SetDashPattern2Async() =>
        TestInputAsync("[1 2] 3 d",
            new DashArrayMock(3, new double[] { 1, 2 }));
    
    [Fact]
    public Task SetDashPattern1Async() =>
        TestInputAsync("[1] 3 d",
            new DashArrayMock(3, new double[] { 1 }));
    
    [Fact]
    public Task SetDashPattern0Async() =>
        TestInputAsync("[] 3 d", new DashArrayMock(3, new double[0]));

    [Fact]
    public Task ParseRenderingIntentAsync() =>
        TestInputAsync("/Perceptual ri", i => i.SetRenderIntent(RenderIntentName.Perceptual));

    [Fact]
    public Task ParseFlatnessToleranceAsync() =>
            TestInputAsync("27 i", i => i.SetFlatnessTolerance(27));
    [Fact]
    public Task LoadGsDictionaryAsync() => 
        TestInputAsync("/JdmState gs", i => i.LoadGraphicStateDictionaryAsync(PdfDirectValue.CreateName("JdmState")));
}