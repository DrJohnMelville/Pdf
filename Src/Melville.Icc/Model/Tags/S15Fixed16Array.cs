using System.Buffers;
using System.Security.Cryptography.X509Certificates;
using Melville.Icc.Parser;

namespace Melville.Icc.Model.Tags;

public class S15Fixed16Array : ProfileData
{
    public IReadOnlyList<float> Values { get; }
    public S15Fixed16Array(ref SequenceReader<byte> reader)
    {
        reader.Skip32BitPad();
        var values = new float[(reader.Sequence.Length- 8) / 4];
        for (int i = 0; i < values.Length; i++)
        {
            values[i] = reader.Reads15Fixed16();
        }
        Values = values;
    }
}