using Melville.Icc.Model.Tags;
using Xunit;

namespace Melville.Pdf.DataModelTests.ICC;

public class GenericLutTest
{
    public static readonly ICurveTag[] NullCurveTripple =
    {
        new ParametricCurveTag(1,1,0,0,-1000,1,0),
        new ParametricCurveTag(1,1,0,0,-1000,1,0),
        new ParametricCurveTag(1,1,0,0,-1000,1,0)
    };

    private static readonly ICurveTag[] MutatingCurveSet = new ICurveTag[]
    {
        new ParametricCurveTag(1, 2, 0, 0, -1000, 1, 0),
        new ParametricCurveTag(1, 3, 0, 0, -1000, 1, 0),
        new ParametricCurveTag(1, 4, 0, 0, -1000, 1, 0),
    };

    [Fact]
    public void MatrixRotationAB() =>
        ColorTransformTestHelpers.VerifyMatrixTripple(new LutAToBTag(NullCurveTripple, NullColorTransform.Instance(3), NullCurveTripple,
                new AugmentedMatrix3x3(new Matrix3x3(0, 0, 1, 0, 2, 0, 1, 0, 0), 7,8,9), NullCurveTripple), 
            10, 12, 10);

    [Fact]
    public void MatrixRotationBA() =>
        ColorTransformTestHelpers.VerifyMatrixTripple(new LutBToATag(NullCurveTripple, NullColorTransform.Instance(3), NullCurveTripple,
                new AugmentedMatrix3x3(new Matrix3x3(0, 0, 1, 0, 2, 0, 1, 0, 0), 7,8,9), NullCurveTripple),
            10, 12, 10);

    [Fact]
    public void InputCurveWorksA()
    {
        var tag = new LutAToBTag(MutatingCurveSet, NullColorTransform.Instance(3), NullCurveTripple, AugmentedMatrix3x3.Identity,
            NullCurveTripple);
        ColorTransformTestHelpers.VerifyMatrixTripple(tag, 2, 6, 12);
    }
    [Fact]
    public void InputCurveWorksB()
    {
        var tag = new LutBToATag(MutatingCurveSet, NullColorTransform.Instance(3), NullCurveTripple, AugmentedMatrix3x3.Identity,
            NullCurveTripple);
        ColorTransformTestHelpers.VerifyMatrixTripple(tag, 2, 6, 12);
    }
    [Fact]
    public void OutputCurveWorksA()
    {
        var tag = new LutAToBTag(NullCurveTripple, NullColorTransform.Instance(3), NullCurveTripple, AugmentedMatrix3x3.Identity,
            MutatingCurveSet);
        ColorTransformTestHelpers.VerifyMatrixTripple(tag, 2, 6, 12);
    }
    [Fact]
    public void OutputCurveWorksB()
    {
        var tag = new LutBToATag(NullCurveTripple, NullColorTransform.Instance(3), NullCurveTripple, AugmentedMatrix3x3.Identity,
            MutatingCurveSet);
        ColorTransformTestHelpers.VerifyMatrixTripple(tag, 2, 6, 12);
    }
    [Fact]
    public void MatrixCurveWorksA()
    {
        var tag = new LutAToBTag(NullCurveTripple, NullColorTransform.Instance(3), MutatingCurveSet, AugmentedMatrix3x3.Identity,
            NullCurveTripple);
        ColorTransformTestHelpers.VerifyMatrixTripple(tag, 2, 6, 12);
    }
    [Fact]
    public void MatrixCurveWorksB()
    {
        var tag = new LutBToATag(NullCurveTripple, NullColorTransform.Instance(3), MutatingCurveSet, AugmentedMatrix3x3.Identity,
            NullCurveTripple);
        ColorTransformTestHelpers.VerifyMatrixTripple(tag, 2, 6, 12);
    }

    [Fact]
    public void ClutWorksA()
    {
        var tag = new LutAToBTag(NullCurveTripple, new StubColorTransformation(), NullCurveTripple, AugmentedMatrix3x3.Identity,
            NullCurveTripple);
        ColorTransformTestHelpers.VerifyMatrixTripple(tag, 6, 16, 42);
    }
    [Fact]
    public void ClutWorksB()
    {
        var tag = new LutBToATag(NullCurveTripple, new StubColorTransformation(), NullCurveTripple, AugmentedMatrix3x3.Identity,
            NullCurveTripple);
        ColorTransformTestHelpers.VerifyMatrixTripple(tag, 6, 16, 42);
    }
    
    [Theory]
    [InlineData(2,1,0,0,-10,0,0,2,4)]
    [InlineData(2,1,0,0,-10,0,0,3,9)]
    [InlineData(1,5,0,0,-10,0,0,3,15)]
    [InlineData(1,1,4,0,-10,0,0,3,7)]
    [InlineData(2,1,2,2,-10,0,0,3,27)]
    [InlineData(2,1,2,2,10,0,13,3,13)]
    [InlineData(0,0,0,0,10,5,0,3,15)]
    public void ParametricCurve(
        float G, float A, float B, float C, float D, float E, float F, float input, float output)
    {
        Assert.Equal(output, new ParametricCurveTag(G, A, B, C, D, E, F).Evaluate(input));
        
    }
}