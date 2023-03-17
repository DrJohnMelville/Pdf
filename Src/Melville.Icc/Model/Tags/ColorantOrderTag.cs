using System.Buffers;
using Melville.Icc.Parser;
using Melville.Parsing.SequenceReaders;

namespace Melville.Icc.Model.Tags;

/// <summary>
/// This tag represents the order in which colorants are laid down in a multi-color device.  Indexes in this table refer to the
/// ColorantTableTag
/// </summary>
public class ColorantOrderTag 
{
    /// <summary>
    /// A list of bytes representing the colorant order in a device color space.
    /// </summary>
    public IReadOnlyList<byte> Colors { get; }

    internal ColorantOrderTag(ref SequenceReader<byte> data)
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