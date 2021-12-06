using System.Buffers;
using Melville.Icc.Parser;

namespace Melville.Icc.Model.Tags;

public class ChromacityTag 
{
    public ushort Channels { get; }
    public Colorant Colorant { get; }
    public (float X, float Y)[] Coordinates { get; } 
    public ChromacityTag(ref SequenceReader<byte> reader)
    {
        reader.ReadBigEndianUint32(); // throw away padding
        Channels = reader.ReadBigEndianUint16();
        Colorant = (Colorant)reader.ReadBigEndianUint16();
        Coordinates = new (float, float)[Channels];
        for (int i = 0; i < Channels; i++)
        {
            Coordinates[i] = (reader.Readu16Fixed16(), reader.Readu16Fixed16());
        }
    }
}