using System.Buffers;
using Melville.Icc.ColorTransforms;

namespace Melville.Icc.Model.Tags;

public class LutBToATag : GenericLut
{
    internal LutBToATag(ICurveTag[] inputCurves, IColorTransform lookupTable, ICurveTag[] matrixCurves, 
        AugmentedMatrix3x3 matrix, ICurveTag[] outputCurves) : base(inputCurves, matrixCurves, matrix, outputCurves, lookupTable)
    {
    }

    internal LutBToATag(ref SequenceReader<byte> reader): base(ref reader, true)
    {
    }

    /// <inheritdoc />
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