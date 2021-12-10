using System.Buffers;

namespace Melville.Icc.Model.Tags;

public class LutBToATag : GenericLut
{
    public LutBToATag(ICurveTag[] inputCurves, IColorTransform lookupTable, ICurveTag[] matrixCurves, 
        AugmentedMatrix3x3 matrix, ICurveTag[] outputCurves) : base(inputCurves, matrixCurves, matrix, outputCurves, lookupTable)
    {
    }

    public LutBToATag(ref SequenceReader<byte> reader): base(ref reader, true)
    {
    }

    public override void Transform(in ReadOnlySpan<float> input, in Span<float> output)
    {
        this.VerifyTransform(input, output);
        Span<float> localInput = stackalloc float[input.Length];
        CurveTransform(InputCurves, input, localInput);
        if (localInput.Length == 3) Matrix.PostMultiplyBy(localInput, localInput);
        CurveTransform(MatrixCurves, localInput, localInput);
        LookupTable.VerifyTransform(localInput, output);
        LookupTable.Transform(localInput, output);
        CurveTransform(OutputCurves, output, output);
    }
}