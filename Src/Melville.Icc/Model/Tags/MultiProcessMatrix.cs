using System.Buffers;
using Melville.Icc.Parser;

namespace Melville.Icc.Model.Tags;

public class MultiProcessMatrix : IMultiProcessElement
{
    public int Inputs { get; }
    public int Outputs { get; }
    public IReadOnlyList<float> Values;

    public MultiProcessMatrix(ref SequenceReader<byte> reader)
    {
        reader.Skip32BitPad();
        Inputs = reader.ReadBigEndianUint16();
        Outputs = reader.ReadBigEndianUint16();
        Values = reader.ReadIEEE754FloatArray((Inputs * Outputs) + Outputs);
    }
}