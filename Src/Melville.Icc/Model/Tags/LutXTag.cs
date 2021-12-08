using System.Buffers;
using Melville.Icc.Parser;

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

    public LutXTag(ref SequenceReader<byte> reader, int teblePrecisionInBytes)
    {
        reader.Skip32BitPad();
        Inputs = reader.ReadBigEndianUint8();
        Outputs = reader.ReadBigEndianUint8();
        GridPoints = reader.ReadBigEndianUint8();
        reader.Skip8BitPad();
        Matrix = new Matrix3x3(ref reader);
        InputTableEntries = reader.ReadBigEndianUint16();
        OutputTableEntries = reader.ReadBigEndianUint16();
        inputTables = reader.ReadScaledFloatArray(InputTableEntries * Inputs,teblePrecisionInBytes);
        clut = reader.ReadScaledFloatArray((int)Math.Pow(GridPoints, Inputs) * Outputs,teblePrecisionInBytes);
        outputTables = reader.ReadScaledFloatArray(OutputTableEntries * Outputs,teblePrecisionInBytes);
        clutTransform = ComputeClutTransform();
    }

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
        TryInputTransform(intermed);
        clutTransform.Transform(intermed, output);
        TryOutputTransform(output);
    }

    private void TryMatrixMultiplication(in ReadOnlySpan<float> input, in Span<float> intermed)
    {
        if (input.Length == 3)
            Matrix.PostMultiplyBy(input, intermed);
        else
            input.CopyTo(intermed);
    }

    private void TryInputTransform(in Span<float> intermed)
    {
        if (InputTableEntries > 0) 
            OneDimensionalLookupTable.MultiLookup(intermed, inputTables.AsSpan());
    }

    private void TryOutputTransform(in Span<float> output)
    {
        if (OutputTableEntries > 0) 
            OneDimensionalLookupTable.MultiLookup(output, outputTables.AsSpan());
    }
}