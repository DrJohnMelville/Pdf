using Melville.Fonts.SfntParsers.TableDeclarations.CffGlyphs;
using Melville.Parsing.CountingReaders;
using Melville.Parsing.IStreamAdaptors;
using Melville.Parsing.MultiplexSources;
using Melville.StreamInterfaces;
using System.Buffers;
using System.Diagnostics;

namespace Melville.Fonts.SfntParsers.TableDeclarations.CFF2Glyphs;

internal readonly struct PrivateCff2DictionaryParser(
    IMultiplexSource source, IByteSource pipe, IFontDictSelector selector)
{
    public async ValueTask<IFontDictExecutorSelector> ParseAsync()
    {
        var count = (int) await pipe.ReadBigEndianUintAsync(4).CA();
        if (count == 0) return NullExecutorSelector.Instance;
        var offsetSize = (int)await pipe.ReadBigEndianUintAsync(1).CA();

        using var offsets = new PooledBuffer<int>(count*2);

        var first = (int)await pipe.ReadBigEndianUintAsync(offsetSize).CA();
        Debug.Assert(first == 1);
        for (int i = 0; i < count; i++)
        {
            ((int[])offsets)[i] = (int)await pipe.ReadBigEndianUintAsync(offsetSize).CA();
        }

        for(int i = 0; i < count; i++)
        {
            var length = offsets[i] - first;
            first = offsets[i];
            var sequence = await pipe.ReadAtLeastAsync(length).CA();
            var trimmed = sequence.Buffer.Slice(0, length);
            (offsets[i + count], offsets[i]) = ParseFontDict(trimmed);
            pipe.AdvanceTo(trimmed.End);
        }

        for (int i = 0; i < count; i++)
        {
            using var p2 = source.ReadPipeFrom((uint)offsets[i]);
            var result = await p2.ReadAtLeastAsync(offsets[count+ i]).CA();
            var trimmed = result.Buffer.Slice(0, offsets[count + i]);
            offsets[i+count] = ReadPrivateDict(trimmed);
            p2.AdvanceTo(trimmed.End);
        }

        using var procIndexes = new PooledBuffer<CffIndex>(count);
        for (int i = 0; i < count; i++)
        {
            if (offsets[i + count] == 0)
            {
                ((CffIndex[])procIndexes)[i] = new CffIndex(source, 0, 0, null);
                continue;
            }

            var indexOffset = offsets[i]+offsets[i+count];
            using var  p2 = source.ParsingBufferFrom(indexOffset);
            ((CffIndex[])procIndexes)[i] = await new CFFIndexParser(source.OffsetFrom(
                    (uint)indexOffset), p2)
                .ParseCff2Async().CA();
        }

        var ret = RealizeSelector(selector, procIndexes.AsSpan(0, count));
        return ret;
    }


    private IFontDictExecutorSelector RealizeSelector(IFontDictSelector sel,
        ReadOnlySpan<CffIndex> indexes)
    {
        var executors = ArrayPool<IGlyphSubroutineExecutor>.Shared.Rent(indexes.Length);
        for (int i = 0; i < indexes.Length; i++)
        {
            executors[i] = new GlyphSubroutineExecutor(indexes[i]);
        }

        var ret = sel.GetSelector(executors.AsSpan(0, indexes.Length));
        ArrayPool<IGlyphSubroutineExecutor>.Shared.Return(executors);
        return ret;
    }

    private int ReadPrivateDict(ReadOnlySequence<byte> trimmed)
    {
        var reader = new SequenceReader<byte>(trimmed);
        Span<DictValue> operands = stackalloc DictValue[1];
        var dictReader = new DictParser<CffDictionaryDefinition>(
            reader, null, operands);
        if (!dictReader.TryFindEntry(0x13)) return 0;
        return operands[0].IntValue;
    }

    private (int, int) ParseFontDict(ReadOnlySequence<byte> source)
    {
        var reader = new SequenceReader<byte>(source);
        Span<DictValue> operands = stackalloc DictValue[2];
        var dictReader = new DictParser<CffDictionaryDefinition>(
            reader, null, operands);
        if (!dictReader.TryFindEntry(0x12)) return (0, 0);
        return (operands[0].IntValue, operands[1].IntValue);
    }
}