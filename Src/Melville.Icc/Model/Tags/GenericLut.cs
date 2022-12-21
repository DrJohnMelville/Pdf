using System.Buffers;
using System.Diagnostics;
using Melville.Icc.ColorTransforms;
using Melville.Icc.Parser;
using Melville.Parsing.SequenceReaders;

namespace Melville.Icc.Model.Tags;

/// <summary>
/// This is a base class which represents the LutAToB and LutBToA Tags.  These two tags are parsed identically but computed
/// in different orders.
/// </summary>
public abstract class GenericLut: IColorTransform 
{
    private readonly ICurveTag[] inputCurves;
    public IReadOnlyList<ICurveTag> InputCurves => inputCurves;

    private readonly ICurveTag[] matrixCurves;
    public IReadOnlyList<ICurveTag> MatrixCurves => matrixCurves;

    public AugmentedMatrix3x3 Matrix { get; }

    private readonly ICurveTag[] outputCurves;
    public IReadOnlyList<ICurveTag> OutputCurves => outputCurves;

    public IColorTransform LookupTable { get; }

    private protected GenericLut(ICurveTag[] inputCurves, ICurveTag[] matrixCurves, AugmentedMatrix3x3 matrix, ICurveTag[] outputCurves, IColorTransform lookupTable)
    {
        this.inputCurves = inputCurves;
        this.matrixCurves = matrixCurves;
        this.Matrix = matrix;
        this.outputCurves = outputCurves;
        LookupTable = lookupTable;
    }

    private protected GenericLut(ref SequenceReader<byte> reader, bool bIsInput)
    {
        reader.VerifyInCorrectPositionForTagRelativeOffsets();
        reader.Skip32BitPad();
        var inputs = new ICurveTag[reader.ReadBigEndianUint8()];
        var outputs = new ICurveTag[reader.ReadBigEndianUint8()];
        var matrixCurves = new ICurveTag[3];
        inputCurves = inputs;
        this.matrixCurves = matrixCurves;
        outputCurves = outputs;
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

    /// <inheritdoc />
    public int Inputs => inputCurves.Length;

    /// <inheritdoc />
    public int Outputs => outputCurves.Length;

    /// <inheritdoc />
    public abstract void Transform(in ReadOnlySpan<float> input, in Span<float> output);

    protected void CurveTransform(
        IReadOnlyList<ICurveTag> curves, in ReadOnlySpan<float> input, in Span<float> output)
    {
        Debug.Assert(curves.Count == input.Length);
        Debug.Assert(curves.Count == output.Length);
        for (int i = 0; i < input.Length; i++)
        {
            output[i] = curves[i].Evaluate(input[i]);
        }
    }
}