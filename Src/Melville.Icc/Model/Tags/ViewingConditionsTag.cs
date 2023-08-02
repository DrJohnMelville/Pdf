using System.Buffers;
using Melville.Icc.Parser;
using Melville.Parsing.SequenceReaders;

namespace Melville.Icc.Model.Tags;

/// <summary>
/// ICC tag describing the viewing conditions in which a color measurement was taken
/// </summary>
public class ViewingConditionsTag 
{ 
    /// <summary>
    /// CIE XYZ values for the color of the illuminant
    /// </summary>
    public XyzNumber IlluminantValue { get; }
    /// <summary>
    /// CIE XYZ values for the surrounding color
    /// </summary>
    public XyzNumber SurroundValue { get; }
    /// <summary>
    /// The illuminant type for this viewing.
    /// </summary>
    public StandardIllumination IlluminantType { get; }
    internal ViewingConditionsTag(ref SequenceReader<byte> reader)
    {
        reader.Skip32BitPad();
        IlluminantValue = reader.ReadXyzNumber();
        SurroundValue = reader.ReadXyzNumber();
        IlluminantType = (StandardIllumination)reader.ReadBigEndianUint32();
    }
}