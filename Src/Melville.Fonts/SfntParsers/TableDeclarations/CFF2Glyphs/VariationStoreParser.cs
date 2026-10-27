using Melville.StreamInterfaces;
using Melville.StreamInterfaces.ParsingBuffers;
using System.Buffers.Binary;
using System.Diagnostics;

namespace Melville.Fonts.SfntParsers.TableDeclarations.CFF2Glyphs;

internal readonly struct VariationStoreParser(ParsingBuffer buffer)
{
    public async ValueTask<uint[]> ParseAsync()
    {
        var size = await buffer.GetUInt16BigEndianAsync().CA();

        return Parse((await buffer.TakeBytesAsync(size).CA()).Span);
    }

    private  uint[] Parse(Span<byte> buffer)
    {
        var reader = new SpanParsingBuffer(buffer);
        var format = reader.GetUInt16BigEndian();
        Debug.Assert(format == 1);

        var variationRegionOffset = reader.GetUInt32BigEndian();
        var variationDataCount = reader.GetUInt16BigEndian();
        var ret = new uint[variationDataCount];
        for (int i = 0; i < variationDataCount; i++)
        {
            ret[i] = reader.GetUInt32BigEndian();
        }

        for (int i = 0; i < variationDataCount; i++)
        {
            ret[i] = BinaryPrimitives.ReadUInt16BigEndian(buffer[(i + 4)..]);
        }


        return ret;
    }

}