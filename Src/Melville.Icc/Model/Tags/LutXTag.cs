using System.Buffers;
using Melville.Icc.Parser;

namespace Melville.Icc.Model.Tags;

public class LutXTag : ProfileData
{
    public Matrix3x3 Matrix { get; }
    public byte Inputs { get; }
    public byte Outputs { get; }
    public byte GridPoints { get; }
    public ushort InputTableEntries { get; }
    public ushort OutputTableEntries { get; }
    public IReadOnlyList<float> InputTables { get; }
    public IReadOnlyList<float> Clut { get; }
    public IReadOnlyList<float> OutputTables { get; }
    public LutXTag(ref SequenceReader<byte> reader, int teblePrecisionInBytes)
    {
        reader.ReadBigEndianUint32(); // padding
        Inputs = reader.ReadBigEndianUint8();
        Outputs = reader.ReadBigEndianUint8();
        GridPoints = reader.ReadBigEndianUint8();
        reader.ReadBigEndianUint8(); // padding
        Matrix = new Matrix3x3(
            reader.Reads15Fixed16(),
            reader.Reads15Fixed16(),
            reader.Reads15Fixed16(),
            reader.Reads15Fixed16(),
            reader.Reads15Fixed16(),
            reader.Reads15Fixed16(),
            reader.Reads15Fixed16(),
            reader.Reads15Fixed16(),
            reader.Reads15Fixed16()
        );
        InputTableEntries = reader.ReadBigEndianUint16();
        OutputTableEntries = reader.ReadBigEndianUint16();
        InputTables = reader.ReadScaledFloatArray(InputTableEntries * Inputs,teblePrecisionInBytes);
        Clut = reader.ReadScaledFloatArray((int)Math.Pow(GridPoints, Inputs) * Outputs,teblePrecisionInBytes);
        OutputTables = reader.ReadScaledFloatArray(OutputTableEntries * Outputs,teblePrecisionInBytes);
    }
}