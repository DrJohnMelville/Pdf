using System.Buffers;

namespace Melville.Icc.Model.Tags;

public class LutBToATag : GenericLut
{
    public LutBToATag(ref SequenceReader<byte> reader): base(ref reader, true)
    {
    }
}