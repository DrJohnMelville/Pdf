using System.Buffers;
using Melville.Icc.ColorTransforms;
using Melville.Icc.Parser;
using Melville.Parsing.SequenceReaders;

namespace Melville.Icc.Model.Tags;

/// <summary>
/// Represents the Lut8 and LUT16 tags in the ICC spec.  These two tags differ only by the precision of the constants use.
/// This distinction is lost after parsing in this implementation.  Given a LutXTag you cannot figure out if it was created from
/// 8 or 16 bit data
/// </summary>
public class LutXTag: IColorTransform 
{
    /// <summary>
    /// The transformation matrix
    /// </summary>
    public Matrix3x3 Matrix { get; }

    /// <inheritdoc />
    public int Inputs { get; }

    /// <inheritdoc />
    public int Outputs { get; }
    /// <summary>
    /// Number of grid points in each dimension of the lookup table.
    /// </summary>
    public byte GridPoints { get; }

    /// <summary>
    /// Number of complete color entries in the input table.
    /// </summary>
    public ushort InputTableEntries { get; }
    /// <summary>
    /// Number of complete color entries in the output table.
    /// </summary>
    public ushort OutputTableEntries { get; }

    private readonly float[] inputTables;
    /// <summary>
    /// Input table entries
    /// </summary>
    public IReadOnlyList<float> InputTables => inputTables;

    private readonly float[] clut;
    /// <summary>
    /// Look up table for the transformation
    /// </summary>
    public IReadOnlyList<float> Clut => clut;

    private readonly float[] outputTables;
    /// <summary>
    /// Output transformation tables
    /// </summary>
    public IReadOnlyList<float> OutputTables => outputTables;

    private IColorTransform clutTransform;

    internal LutXTag(byte inputs, byte outputs, float[] inputTables, 
         Matrix3x3 matrix,
        float[] clut, float[] outputTables)
    {
        Matrix = matrix;
        Inputs = inputs;
        Outputs = outputs;
        GridPoints = (byte)(clut.Length / (inputs*outputs));
        InputTableEntries = (ushort)(inputTables.Length/inputs);
        OutputTableEntries = (ushort)(outputTables.Length/outputs);
        this.inputTables = inputTables;
        this.clut = clut;
        this.outputTables = outputTables;
        clutTransform = ComputeClutTransform();
    }

    internal LutXTag(ref SequenceReader<byte> reader, int tablePrecisionInBytes)
    {
        reader.Skip32BitPad();
        Inputs = reader.ReadBigEndianUint8();
        Outputs = reader.ReadBigEndianUint8();
        GridPoints = reader.ReadBigEndianUint8();
        reader.Skip8BitPad();
        Matrix = new Matrix3x3(ref reader);

        InputTableEntries = GetTableLength(tablePrecisionInBytes, ref reader);
        OutputTableEntries = GetTableLength(tablePrecisionInBytes, ref reader);
        
        inputTables = reader.ReadScaledFloatArray(InputTableEntries * Inputs,tablePrecisionInBytes);
        clut = reader.ReadScaledFloatArray((int)Math.Pow(GridPoints, Inputs) * Outputs,tablePrecisionInBytes);
        outputTables = reader.ReadScaledFloatArray(OutputTableEntries * Outputs,tablePrecisionInBytes);
        clutTransform = ComputeClutTransform();
    }

    // the 16 bit version encodes the input and output table length but set to 256 in the 8 bit version
    private ushort GetTableLength(int tablePrecisionInBytes, ref SequenceReader<byte> reader) =>
        tablePrecisionInBytes == 1 ? (ushort)256 : reader.ReadBigEndianUint16();

    private IColorTransform ComputeClutTransform()
    {
        return GridPoints == 0
            ? TrivialClut()
            : new MultidimensionalLookupTable(
                Enumerable.Repeat((int)GridPoints, Inputs).ToArray(), Outputs, clut);
    }

    private IColorTransform TrivialClut()
    {
        if (Inputs != Outputs)
            throw new InvalidDataException("LUTXTag must have a CLUT or inputs and outputs must be same");
        return NullColorTransform.Instance(Inputs);
    }

    /// <inheritdoc />
    public void Transform(in ReadOnlySpan<float> input, in Span<float> output)
    {
        this.VerifyTransform(input, output);
        Span<float> intermed = stackalloc float[input.Length];
        TryMatrixMultiplication(input, intermed);
        OneDimensionalLookupTable.LookupInPlace(intermed, inputTables.AsSpan());
        clutTransform.Transform(intermed, output);
        OneDimensionalLookupTable.LookupInPlace(output, outputTables.AsSpan());
    }

    private void TryMatrixMultiplication(in ReadOnlySpan<float> input, in Span<float> intermed)
    {
        if (input.Length == 3)
            Matrix.PostMultiplyBy(input, intermed);
        else
            input.CopyTo(intermed);
    }
}