using System.Buffers;
using System.Diagnostics;
using System.IO.Pipelines;
using Melville.Parsing.AwaitConfiguration;
using Melville.Parsing.CountingReaders;
using Melville.Parsing.PipeReaders;
using Melville.Parsing.SequenceReaders;

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
        var ret = reader.ReadBigEndianUint16();
        pipe.AdvanceTo(reader.Position);
        return ret;
    }

    private  uint[] Parse(ReadOnlySequence<byte> buffer)
    {
        var reader = new SequenceReader<byte>(buffer);
        var format = reader.ReadBigEndianUint16();
        Debug.Assert(format == 1);
        var variationRegionOffset = reader.ReadBigEndianUint32();
        var variationDataCount = reader.ReadBigEndianUint16();
        var ret = new uint[variationDataCount];
        for (int i = 0; i < variationDataCount; i++)
        {
            ret[i] = reader.ReadBigEndianUint32();
        }

        for (int i = 0; i < variationDataCount; i++)
        {
            reader = new SequenceReader<byte>(buffer.Slice(ret[i]+4));
            ret[i] = reader.ReadBigEndianUint16();
        }

        return ret;
    }

}