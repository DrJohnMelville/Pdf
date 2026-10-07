using System.Buffers;
using Melville.Parsing.SequenceReaders;
using Melville.StreamInterfaces.ParsingBuffers;

namespace Melville.Icc.Model.Tags;

/// <summary>
/// Describes a single colorant used in a multi-colorant output device
/// </summary>
/// <param name="Name">Name of the colorant</param>
/// <param name="X">X component of the colorant value in XYZ notation</param>
/// <param name="Y">Y component of the colorant value in XYZ notation</param>
/// <param name="Z">Z component of the colorant value in XYZ notation</param>
public record struct ColorantTableEntry(string Name, ushort X, ushort Y, ushort Z);
/// <summary>
/// A tag representing a table of colorants for the described device.
/// </summary>
public class ColorantTableTag 
{
    /// <summary>
    /// List of colorants used by the described device, not necessarily in the order they are used.
    /// </summary>
    public IReadOnlyList<ColorantTableEntry> Colorants;

    internal ColorantTableTag(ref SpanParsingBuffer reader)
    {
        reader.Skip32BitPad();
        var cols = new ColorantTableEntry[reader.GetUInt32BigEndian()];
        for (int i = 0; i < cols.Length; i++)
        {
            cols[i] = new ColorantTableEntry(reader.ReadFixedAsciiString(32),
                reader.GetUInt16BigEndian(), reader.GetUInt16BigEndian(), reader.GetUInt16BigEndian());
        }
        Colorants = cols;
    }
}