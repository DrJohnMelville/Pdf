using System.Buffers;
using Melville.Parsing.SequenceReaders;

namespace Melville.Fonts.SfntParsers.TableParserParts;

internal struct UInt24
{
    private UInt24(uint data) => this.data = data;
    private uint data;

    public static implicit operator UInt24(uint data) => new(data);
    public static implicit operator uint(UInt24 data) => data.data;
    public static implicit operator int(UInt24 data) => (int)data.data;

    public static bool TryReadBigEndian(ref SequenceReader<byte> reader, out UInt24 output)
    {
        if (reader.TryReadBigEndian(out ulong data, 3))
        {
            output = (uint)data;
            return true;
        }

        output = 0;
        return false;
    }
}