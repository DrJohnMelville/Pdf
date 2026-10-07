using System.Buffers;
using Melville.Parsing.SequenceReaders;
using Melville.StreamInterfaces.ParsingBuffers;

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

    internal ColorantOrderTag(ref SpanParsingBuffer data)
    {
        data.Skip32BitPad();
        var cols = new byte[data.GetUInt32BigEndian()];
        for (int i = 0; i < cols.Length; i++)
        {
            cols[i] = data.GetUInt8();
        }

        Colors = cols;
    }
}