using Melville.Icc.Model.Tags;

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

    [Test]
    public void MatrixRotationAB() =>
        ColorTransformTestHelpers.VerifyMatrixTripple(new LutAToBTag(NullCurveTripple, NullColorTransform.Instance(3), NullCurveTripple,
                new AugmentedMatrix3x3(new Matrix3x3(0, 0, 1, 0, 2, 0, 1, 0, 0), 7,8,9), NullCurveTripple), 
            10, 12, 10);

    [Test]
    public void MatrixRotationBA() =>
        ColorTransformTestHelpers.VerifyMatrixTripple(new LutBToATag(NullCurveTripple, NullColorTransform.Instance(3), NullCurveTripple,
                new AugmentedMatrix3x3(new Matrix3x3(0, 0, 1, 0, 2, 0, 1, 0, 0), 7,8,9), NullCurveTripple),
            10, 12, 10);

    [Test]
    public void InputCurveWorksA()
    {
        var tag = new LutAToBTag(MutatingCurveSet, NullColorTransform.Instance(3), NullCurveTripple, AugmentedMatrix3x3.Identity,
            NullCurveTripple);
        ColorTransformTestHelpers.VerifyMatrixTripple(tag, 2, 6, 12);
    }
    [Test]
    public void InputCurveWorksB()
    {
        var tag = new LutBToATag(MutatingCurveSet, NullColorTransform.Instance(3), NullCurveTripple, AugmentedMatrix3x3.Identity,
            NullCurveTripple);
        ColorTransformTestHelpers.VerifyMatrixTripple(tag, 2, 6, 12);
    }
    [Test]
    public void OutputCurveWorksA()
    {
        var tag = new LutAToBTag(NullCurveTripple, NullColorTransform.Instance(3), NullCurveTripple, AugmentedMatrix3x3.Identity,
            MutatingCurveSet);
        ColorTransformTestHelpers.VerifyMatrixTripple(tag, 2, 6, 12);
    }
    [Test]
    public void OutputCurveWorksB()
    {
        var tag = new LutBToATag(NullCurveTripple, NullColorTransform.Instance(3), NullCurveTripple, AugmentedMatrix3x3.Identity,
            MutatingCurveSet);
        ColorTransformTestHelpers.VerifyMatrixTripple(tag, 2, 6, 12);
    }
    [Test]
    public void MatrixCurveWorksA()
    {
        var tag = new LutAToBTag(NullCurveTripple, NullColorTransform.Instance(3), MutatingCurveSet, AugmentedMatrix3x3.Identity,
            NullCurveTripple);
        ColorTransformTestHelpers.VerifyMatrixTripple(tag, 2, 6, 12);
    }
    [Test]
    public void MatrixCurveWorksB()
    {
        var tag = new LutBToATag(NullCurveTripple, NullColorTransform.Instance(3), MutatingCurveSet, AugmentedMatrix3x3.Identity,
            NullCurveTripple);
        ColorTransformTestHelpers.VerifyMatrixTripple(tag, 2, 6, 12);
    }

    [Test]
    public void ClutWorksA()
    {
        var tag = new LutAToBTag(NullCurveTripple, new StubColorTransformation(), NullCurveTripple, AugmentedMatrix3x3.Identity,
            NullCurveTripple);
        ColorTransformTestHelpers.VerifyMatrixTripple(tag, 6, 16, 42);
    }
    [Test]
    public void ClutWorksB()
    {
        var tag = new LutBToATag(NullCurveTripple, new StubColorTransformation(), NullCurveTripple, AugmentedMatrix3x3.Identity,
            NullCurveTripple);
        ColorTransformTestHelpers.VerifyMatrixTripple(tag, 6, 16, 42);
    }
    
    [Test]
    [Arguments(2,1,0,0,-10,0,0,2,4)]
    [Arguments(2,1,0,0,-10,0,0,3,9)]
    [Arguments(1,5,0,0,-10,0,0,3,15)]
    [Arguments(1,1,4,0,-10,0,0,3,7)]
    [Arguments(2,1,2,2,-10,0,0,3,27)]
    [Arguments(2,1,2,2,10,0,13,3,13)]
    [Arguments(0,0,0,0,10,5,0,3,15)]
    public void ParametricCurve(
        float G, float A, float B, float C, float D, float E, float F, float input, float output)
    {
        Assert.Equal(output, new ParametricCurveTag(G, A, B, C, D, E, F).Evaluate(input));
        
    }
}