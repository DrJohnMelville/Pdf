using System.Buffers;
using Melville.Icc.Parser;

namespace Melville.Icc.Model.Tags;

public class ColorOrderTag : ProfileData
{
    public IReadOnlyList<byte> Colors { get; }

    public ColorOrderTag(ref SequenceReader<byte> data)
    {
        data.ReadBigEndianUint32(); // discard padding
        var cols = new byte[data.ReadBigEndianUint32()];
        for (int i = 0; i < cols.Length; i++)
        {
            cols[i] = data.ReadBigEndianUint8();
        }

        Colors = cols;
    }
}