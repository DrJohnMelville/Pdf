using System.Buffers;
using System.Runtime.InteropServices.Marshalling;
using System.Security.Cryptography.X509Certificates;
using Melville.Parsing.SequenceReaders;
using Melville.StreamInterfaces.ParsingBuffers;

namespace Melville.Fonts.SfntParsers.TableParserParts;

internal struct UInt24
{
    private UInt24(uint data) => this.data = data;
    private uint data;

    public static implicit operator UInt24(uint data) => new(data);
    public static implicit operator uint(UInt24 data) => data.data;
    public static implicit operator int(UInt24 data) => (int)data.data;

    public static bool TryReadBigEndian(ref SpanParsingBuffer reader, out UInt24 output)
    {

        if (reader.CurrentLength >= 3)
        {
            var span = reader.Peek();
            output = new(((uint)span[0] << 16) | ((uint)span[1] << 8) | span[2]);
            reader.Advance(3);
            return true;
            
        }
        
        output = 0;
        return false;
    }
}