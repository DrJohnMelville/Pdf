using Melville.Parsing.SequenceReaders;
using Melville.StreamInterfaces;
using Melville.StreamInterfaces.ParsingBuffers;

namespace Melville.Fonts.SfntParsers.TableDeclarations.CFF2Glyphs;

internal readonly struct Type34FontSelectorParser(
    ParsingBuffer reader, int glyphSize, int tableSize)
{
    public async ValueTask<IFontDictSelector> ParseAsync()
    {
        var count =(uint) await reader.GetUIntBigEndianAsync(glyphSize).CA();
        var buffer = await reader.TakeBytesAsync((int)(count * (glyphSize + tableSize)) + glyphSize);
        return ParseTable(buffer.Span, count);
    }

    private IFontDictSelector ParseTable(Span<byte> data, uint count)
    {
        var entries = new Type3Entry[count];
        var SpanReader = new SpanParsingBuffer(data);
        SpanReader.ReadBigEndianUint(glyphSize); // throw away the lower bound
        for (int i = 0; i < count; i++)
        {
            var table = SpanReader.ReadBigEndianUint(tableSize);
            var max = SpanReader.ReadBigEndianUint(glyphSize);
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
