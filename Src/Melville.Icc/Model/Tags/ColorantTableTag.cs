using System.Buffers;
using Melville.Icc.Parser;

namespace Melville.Icc.Model.Tags;

public record struct ColorantTableEntry(string Name, ushort X, ushort Y, ushort Z);
public class ColorantTableTag 
{
    public IReadOnlyList<ColorantTableEntry> Colorants;

    public ColorantTableTag(ref SequenceReader<byte> reader)
    {
        reader.ReadBigEndianUint32(); // pasdding
        var cols = new ColorantTableEntry[reader.ReadBigEndianUint32()];
        for (int i = 0; i < cols.Length; i++)
        {
            cols[i] = new ColorantTableEntry(reader.ReadFixedAsciiString(32),
                reader.ReadBigEndianUint16(), reader.ReadBigEndianUint16(), reader.ReadBigEndianUint16());
        }
        Colorants = cols;
    }
}