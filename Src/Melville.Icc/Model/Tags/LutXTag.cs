using System.Buffers;
using Melville.Icc.ColorTransforms;
using Melville.Icc.Parser;
using Melville.Parsing.SequenceReaders;

namespace Melville.Icc.Model.Tags;

public class LutXTag: IColorTransform 
{
    public Matrix3x3 Matrix { get; }
    public int Inputs { get; }
    public int Outputs { get; }
    public byte GridPoints { get; }
    public ushort InputTableEntries { get; }
    public ushort OutputTableEntries { get; }

    private readonly float[] inputTables;
    public IReadOnlyList<float> InputTables => inputTables;

    private readonly float[] clut;
    public IReadOnlyList<float> Clut => clut;

    private readonly float[] outputTables;
    public IReadOnlyList<float> OutputTables => outputTables;

    private IColorTransform clutTransform;

    public LutXTag(byte inputs, byte outputs, float[] inputTables, 
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

    public LutXTag(ref SequenceReader<byte> reader, int tablePrecisionInBytes)
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