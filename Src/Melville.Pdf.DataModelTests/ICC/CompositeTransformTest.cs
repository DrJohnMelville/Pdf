using Melville.Icc.Model;
using Melville.Icc.Model.Tags;
using Xunit;

namespace Melville.Pdf.DataModelTests.ICC;

public class CompositeTransformTest
{
    [Fact]
    public void NullCompositeTransform()
    {
        var comp = NullColorTransform.Instance(3).Concat(NullColorTransform.Instance(3));
        Assert.Equal(3, comp.Inputs);
        Assert.Equal(3, comp.Outputs);
        ColorTransformTestHelpers.VerifyMatrixTripple(comp, 1, 2, 3);
    }
    [Fact]
    public void CompositeTransform() =>
        ColorTransformTestHelpers.VerifyMatrixTripple(
            new StubColorTransformation().Concat(NullColorTransform.Instance(3)), 6, 16,42);

    [Fact]
    public void CompositeTransformReversed() => ColorTransformTestHelpers.VerifyMatrixTripple(
        NullColorTransform.Instance(3).Concat(new StubColorTransformation()), 6, 16,42);

    [Fact]
    public void DoubleCompositeTransform() => ColorTransformTestHelpers.VerifyMatrixTripple(
        new StubColorTransformation().Concat( new StubColorTransformation()),64, 4042,42);
}