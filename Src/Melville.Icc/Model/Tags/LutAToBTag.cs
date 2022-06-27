using System.Buffers;
using Melville.Icc.ColorTransforms;

namespace Melville.Icc.Model.Tags;

public class LutAToBTag : GenericLut
{
    public LutAToBTag(ref SequenceReader<byte> reader): base(ref reader, false)
    {
    }

    public LutAToBTag(ICurveTag[] inputCurves, IColorTransform lookupTable, ICurveTag[] matrixCurves, 
        AugmentedMatrix3x3 matrix, ICurveTag[] outputCurves) : base(inputCurves, matrixCurves, matrix, outputCurves, lookupTable)
    {
    }

    public override void Transform(in ReadOnlySpan<float> input, in Span<float> output)
    {
        this.VerifyTransform(input, output);
        Span<float> localInput = stackalloc float[input.Length];
        CurveTransform(InputCurves, input, localInput);
        LookupTable.VerifyTransform(localInput, output);
        LookupTable.Transform(localInput, output);
        CurveTransform(MatrixCurves, output, output);
        if (output.Length == 3)
        {
            Matrix.PostMultiplyBy(output, output);
        }
        CurveTransform(OutputCurves, output, output);
    }
}