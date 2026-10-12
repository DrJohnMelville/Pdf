using Melville.Parsing.CountingReaders;
using Melville.Parsing.SequenceReaders;
using Melville.StreamInterfaces;
using System.Buffers;
using System.Diagnostics;

namespace Melville.Fonts.SfntParsers.TableDeclarations.CFF2Glyphs;

internal readonly struct VariationStoreParser(IByteSource pipe)
{
    public async ValueTask<uint[]> ParseAsync()
    {
        var result = await pipe.ReadAtLeastAsync(2).CA();
        var count = ReadLength(result.Buffer);
        var tableresult = await pipe.ReadAtLeastAsync(count).CA();
        return Parse(tableresult.Buffer);
    }


    private ushort ReadLength(ReadOnlySequence<byte> resultBuffer)
    {
        var reader = new SequenceReader<byte>(resultBuffer);
        var ret = reader.GetUInt16BigEndian();
        pipe.AdvanceTo(reader.Position);
        return ret;
    }

    private  uint[] Parse(ReadOnlySequence<byte> buffer)
    {
        var reader = new SequenceReader<byte>(buffer);
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
            reader = new SequenceReader<byte>(buffer.Slice(ret[i]+4));
            ret[i] = reader.GetUInt16BigEndian();
        }


        return ret;
    }

}