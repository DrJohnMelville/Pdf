using System.Buffers;
using Melville.Icc.Parser;
using Melville.Parsing.SequenceReaders;

namespace Melville.Icc.Model.Tags;

/// <summary>
/// Describes a single colorant used in a multi-colorant output device
/// </summary>
/// <param name="Name">Name of the colorant</param>
/// <param name="X">X component of the colorant value in XYZ notation</param>
/// <param name="Y">Y component of the colorant value in XYZ notation</param>
/// <param name="Z">Z component of the colorant value in XYZ notation</param>
public record struct ColorantTableEntry(string Name, ushort X, ushort Y, ushort Z);
public class ColorantTableTag 
{
    /// <summary>
    /// List of colorants used by the described device, not necessarially in the order they are used.
    /// </summary>
    public IReadOnlyList<ColorantTableEntry> Colorants;

    internal ColorantTableTag(ref SequenceReader<byte> reader)
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