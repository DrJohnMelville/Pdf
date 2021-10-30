using System;
using System.Buffers;
using System.IO;
using System.IO.Pipelines;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.ContentStreams;
using Melville.Pdf.LowLevel.Model.Conventions;
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
        target.Verify(i=>i.SaveGraphicsState());
        target.Verify(i=>i.RestoreGraphicsState());
        target.VerifyNoOtherCalls();
    }
    [Fact]
    public Task ModifyTransformMatrixTest() => TestInput(
        "1.000 2 03 +4 5.5 -6 cm",i=>i.ModifyTransformMatrix(1, 2, 3, 4, 5.5, -6));

    [Fact]
    public Task SetLineWidthTest() => TestInput("256 w", i=>i.SetLineWidth(256));
    
    [Theory]
    [InlineData(LineCap.Butt)]
    [InlineData(LineCap.Round)]
    [InlineData(LineCap.Square)]
    public Task SetLineCap(LineCap cap) => TestInput(
        $"{(int)cap} J", i=>i.SetLineCap(cap));

    [Theory]
    [InlineData(LineJoinStyle.Miter, 0)]
    [InlineData(LineJoinStyle.Round, 1)]
    [InlineData(LineJoinStyle.Bevel, 2)]
    public Task SetLineJoinStyle(LineJoinStyle joinStyle, int num) =>
        TestInput($"{num} j", i => i.SetLineJoinStyle(joinStyle));

    [Fact]
    public Task SetDashPattern2() =>
        TestInput("[1 2] 3 d",
            i => i.TestSetLineDashPattern(3, new double[]{1,2}));

    [Fact]
    public Task SetDashPattern1() =>
        TestInput("[1] 3 d",
            i => i.TestSetLineDashPattern(3, new double[]{1}));

    [Fact]
    public Task SetDashPattern0() =>
        TestInput("[] 3 d",
            i => i.TestSetLineDashPattern(3, new double[0]));

    public class ConcreteCSO: IContentStreamOperations
    {
        public virtual void SaveGraphicsState()
        {
        }

        public virtual void RestoreGraphicsState()
        {
        }

        public virtual void ModifyTransformMatrix(double a, double b, double c, double d, double e, double f)
        {
        }

        public virtual void SetLineWidth(double width)
        {
        }

        public virtual void SetLineCap(LineCap cap)
        {
        }

        public virtual void SetLineJoinStyle(LineJoinStyle cap)
        {
        }

        public virtual void SetMiterLimit(double miter)
        {
        }

        public virtual void TestSetLineDashPattern(double dashPhase, double[] dashArray)
        {
        }

        public void SetLineDashPattern(
            double dashPhase, ReadOnlySpan<double> dashArray) =>
            TestSetLineDashPattern(dashPhase, dashArray.ToArray());
    }
}