using System.Buffers;
using Melville.Parsing.SequenceReaders;

namespace Melville.Icc.Model.Tags;

internal class S15Fixed16Array 
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