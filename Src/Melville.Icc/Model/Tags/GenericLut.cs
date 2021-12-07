using System.Buffers;
using System.Security.Cryptography.X509Certificates;
using Melville.Icc.Parser;

namespace Melville.Icc.Model.Tags;

public class GenericLut 
{
    public AugmentedMatrix3x3 Matrix { get; }
    public IReadOnlyList<ICurveTag> InputCurves { get; }
    public IReadOnlyList<ICurveTag> MatrixCurves { get; }
    public IReadOnlyList<ICurveTag> OutputCurves { get; }
    public IColorTransform LookupTable { get; }
    protected GenericLut(ref SequenceReader<byte> reader, bool bIsInput)
    {
        reader.VerifyInCorrectPositionForTagRelativeOffsets();
        reader.Skip32BitPad();
        var inputs = new ICurveTag[reader.ReadBigEndianUint8()];
        var outputs = new ICurveTag[reader.ReadBigEndianUint8()];
        var matrixCurves = new ICurveTag[3];
        InputCurves = inputs;
        MatrixCurves = matrixCurves;
        OutputCurves = outputs;
        reader.Skip16BitPad();
        ReadCurves(bIsInput ? inputs : outputs, ref reader);
        Matrix = ReadMatrix(ref reader);
        ReadCurves(matrixCurves, ref reader);
        LookupTable = ParseClut(ref reader);
        ReadCurves(bIsInput ? outputs : inputs, ref reader);


    }
    private IColorTransform ParseClut(ref SequenceReader<byte> reader)
    {
        var offset = reader.ReadBigEndianUint32(); // clut
        if (offset == 0) return NullColorTransform.Instance(InputCurves.Count);
        var clutReader = reader.ReaderAt(offset);
        return new MultidimensionalLookupTable(ref clutReader, OutputCurves.Count);
    }

    private void ReadCurves(ICurveTag[] curves, ref SequenceReader<byte> reader)
    {
        var offset = reader.ReadBigEndianUint32();
        if (offset == 0)
            SetNullCurve(curves);
        else
        {
            var curveReader = reader.ReaderAt(offset);
            ParseCurve(curves, ref curveReader);
        }    
    }

    private void ParseCurve(ICurveTag[] curves, ref SequenceReader<byte> reader)
    {
        for (int i = 0; i < curves.Length; i++)
        {
            curves[i] = ParseSingleCurve(ref reader);
        }
    }

    private ICurveTag ParseSingleCurve(ref SequenceReader<byte> reader)
    {
        return TagParser.Parse(ref reader) as ICurveTag ??
               throw new InvalidDataException("Expected an ICC CurveType or ParametricCurveType");
    }

    private static void SetNullCurve(ICurveTag[] curves)
    {
        for (int i = 0; i < curves.Length; i++)
        {
            curves[i] = NullCurve.Instance;
        }
    }

    private AugmentedMatrix3x3 ReadMatrix(ref SequenceReader<byte> reader)
    {
        var offset = reader.ReadBigEndianUint32();
        if (offset < 1) return 
            new AugmentedMatrix3x3(new Matrix3x3(1f, 0f, 0f, 0f, 1f, 0f, 0f, 0f, 1f), 0f, 0f, 0f);
        var pos = reader.ReaderAt(offset);
        return new AugmentedMatrix3x3(ref pos);
    }
}