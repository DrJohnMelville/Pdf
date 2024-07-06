using System.Buffers;
using System.IO.Pipelines;
using Melville.Parsing.AwaitConfiguration;
using Melville.Parsing.SequenceReaders;

namespace Melville.Fonts.SfntParsers.TableDeclarations.CFF2Glyphs;

internal readonly struct Type34FontSelectorParser(
    PipeReader reader, int glyphSize, int tableSize)
{
    public async ValueTask<IFontDictSelector> ParseAsync()
    {
        var result = await reader.ReadAtLeastAsync(glyphSize).CA();
        var count = ReadUInt16(result.Buffer);
        result = await reader.ReadAtLeastAsync((int)(count * (glyphSize+tableSize)) + glyphSize).CA();
        return ParseTable(result.Buffer, count);
    }

    private uint ReadUInt16(ReadOnlySequence<byte> input)
    {
        var seqReader = new SequenceReader<byte>(input);
        var ret = (uint)seqReader.ReadBigEndianUint(glyphSize);
        reader.AdvanceTo(seqReader.Position);
        return ret;
    }
    private IFontDictSelector ParseTable(ReadOnlySequence<byte> data, uint count)
    {
        var entries = new Type3Entry[count];
        var seqReader = new SequenceReader<byte>(data);
        seqReader.ReadBigEndianUint(glyphSize); // throw awaqy the lower bound
        for (int i = 0; i < count; i++)
        {
            var table = seqReader.ReadBigEndianUint(tableSize);
            var max = seqReader.ReadBigEndianUint(glyphSize);
            entries[i] = new Type3Entry((uint)max, (ushort)table);
        } return new Type3FontDictSelector(entries);
    }
}


internal sealed class Type3FontDictSelector(Type3Entry[] entries) : RootFontDictSelector
{
    protected override int SelectIndexFor(uint glyph)
    {
        foreach (var entry in entries)
        {
            if (entry.Max > glyph) return entry.Item;
        }
        return entries[^1].Item;
    }
}

internal record struct Type3Entry(uint Max, ushort Item);
