using System.Buffers;

namespace Melville.Icc.Model.Tags;

public class LutAToBTag : GenericLut
{
    public LutAToBTag(ref SequenceReader<byte> reader): base(ref reader, false)
    {
    }
}