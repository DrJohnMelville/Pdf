using System.Buffers;
using Melville.Icc.Parser;

namespace Melville.Icc.Model.Tags;

public record struct NamedColorElememt(string Name, XyzNumber PcsValue, ushort[] DeviceValue);

public class NamedColorTag 
{
    public uint VendorSpecificFlag { get; }
    public IReadOnlyList<NamedColorElememt> Colors { get; }

    public NamedColorTag(ref SequenceReader<byte> reader)
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