using System.Buffers;

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
}