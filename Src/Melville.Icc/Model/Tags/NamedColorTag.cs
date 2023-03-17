using System.Buffers;
using Melville.Icc.Parser;
using Melville.Parsing.SequenceReaders;

namespace Melville.Icc.Model.Tags;

/// <summary>
/// A NamedColor2Type line from the ICC spec
/// </summary>
/// <param name="Name">Name of the color</param>
/// <param name="PcsValue">PCS coordinates for the color</param>
/// <param name="DeviceValue">Device coordinate for the color</param>
public record struct NamedColorElememt(string Name, XyzNumber PcsValue, ushort[] DeviceValue);

/// <summary>
/// A set of NamedColorToType rows from the ICC spec;0
/// </summary>
public class NamedColorTag 
{
    /// <summary>
    /// Low 16 bits are for ICC use reamaining bits for verdor specific flags.
    /// </summary>
    public uint VendorSpecificFlag { get; }
    /// <summary>
    /// Named colors in the profile.
    /// </summary>
    public IReadOnlyList<NamedColorElememt> Colors { get; }

    internal NamedColorTag(ref SequenceReader<byte> reader)
    {
        reader.Skip32BitPad();
        VendorSpecificFlag = reader.ReadBigEndianUint32();
        var colors = new NamedColorElememt[reader.ReadBigEndianUint32()];
        var colorElements = (int)reader.ReadBigEndianUint32();
        var prefix = reader.ReadFixedAsciiString(32);
        var postFix = reader.ReadFixedAsciiString(32);
        for (int i = 0; i < colors.Length; i++)
        {
            colors[i] = new NamedColorElememt($"{prefix}{reader.ReadFixedAsciiString(32)}{postFix}",
                reader.ReadXyzFromUint16s(), reader.ReadUshortArray(colorElements));
        }

        Colors = colors;
    }
}