using System;
using System.Numerics;
using System.Threading.Tasks;
using Melville.INPC;
using Melville.Pdf.LowLevel.Model.ContentStreams;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.DataModelTests.Standard.S8_4GraphicState;

public partial class GraphicStateParsers: ParserTest
{
    [Test]
    public Task PushGraphicsStateAsync() => TestInputAsync("q", i => i.SaveGraphicsState());

    [Test]
    public Task PopGraphicsStateAsync() => TestInputAsync(
        "Q", i => i.RestoreGraphicsState());

    [Test]
    public Task IgnoreCommendTestAsync() => TestInputAsync(
        "q% this is a q Q q comment \r\n Q", i => i.SaveGraphicsState(), i => i.RestoreGraphicsState());
    
    [Test]
    public async Task CompositeOperatorsWithWhiteSpaceAsync()
    {
        await ParseStringAsync("   q\r\n  Q  ");
        Target.Verify(i => i.SaveGraphicsState());
        Target.Verify(i => i.RestoreGraphicsState());
        Target.VerifyNoOtherCalls();
    }

    [Test]
    public Task ModifyTransformMatrixTestAsync() => TestInputAsync(
        "1.000 2 03 +4 5.5 -6 cm", i => i.ModifyTransformMatrix(new Matrix3x2(1, 2, 3, 4, 5.5f, -6)));

    [Test]
    public Task SetLineWidthTestAsync() => TestInputAsync("256 w", i => i.SetLineWidth(256));

    [Test]
    public Task SetMiterLimitAsync() => 
        TestInputAsync("10 M", i => i.SetMiterLimit(10));

    [Test]
    [Arguments(LineCap.Butt)]
    [Arguments(LineCap.Round)]
    [Arguments(LineCap.Square)]
    public Task SetLineCapAsync(LineCap cap) => TestInputAsync(
        $"{(int)cap} J", i => i.SetLineCap(cap));

    [Test]
    [Arguments(LineJoinStyle.Miter, 0)]
    [Arguments(LineJoinStyle.Round, 1)]
    [Arguments(LineJoinStyle.Bevel, 2)]
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
            Assert.SeqEqual(dots, dashes.ToArray());
            SetCalled();
            
        }
    }
    
    [Test]
    public Task SetDashPattern2Async() =>
        TestInputAsync("[1 2] 3 d",
            new DashArrayMock(3, new double[] { 1, 2 }));
    
    [Test]
    public Task SetDashPattern1Async() =>
        TestInputAsync("[1] 3 d",
            new DashArrayMock(3, new double[] { 1 }));
    
    [Test]
    public Task SetDashPattern0Async() =>
        TestInputAsync("[] 3 d", new DashArrayMock(3, new double[0]));

    [Test]
    public Task ParseRenderingIntentAsync() =>
        TestInputAsync("/Perceptual ri", i => i.SetRenderIntent(RenderIntentName.Perceptual));

    [Test]
    public Task ParseFlatnessToleranceAsync() =>
            TestInputAsync("27 i", i => i.SetFlatnessTolerance(27));
    [Test]
    public Task LoadGsDictionaryAsync() => 
        TestInputAsync("/JdmState gs", i => i.LoadGraphicStateDictionaryAsync(PdfDirectObject.CreateName("JdmState")));
}