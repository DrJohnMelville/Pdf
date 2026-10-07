using System.Buffers;
using Melville.Parsing.SequenceReaders;

namespace Melville.Icc.Model.Tags;

/// <summary>
/// ICC tag that describes the chromicity of a device.
/// </summary>
public class ChromacityTag 
{
    /// <summary>
    /// Number of channels represent by this tag.
    /// </summary>
    public ushort Channels { get; }
    /// <summary>
    /// Phosphor or colorant tyoe
    /// </summary>
    public Colorant Colorant { get; }
    /// <summary>
    /// The chromacity values for each channel
    /// </summary>
    public (float X, float Y)[] Coordinates { get; } 
    internal ChromacityTag(ref SequenceReader<byte> reader)
    {
        reader.GetUInt32BigEndian(); // throw away padding
        Channels = reader.GetUInt16BigEndian();
        Colorant = (Colorant)reader.GetUInt16BigEndian();
        Coordinates = new (float, float)[Channels];
        for (int i = 0; i < Channels; i++)
        {
            Coordinates[i] = (reader.Readu16Fixed16(), reader.Readu16Fixed16());
        }
    }
}