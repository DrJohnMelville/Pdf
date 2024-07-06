using System.Buffers;
using System.Diagnostics;
using System.IO.Pipelines;
using Melville.Fonts.SfntParsers.TableDeclarations.CffGlyphs;
using Melville.INPC;
using Melville.Parsing.AwaitConfiguration;
using Melville.Parsing.MultiplexSources;

namespace Melville.Fonts.SfntParsers.TableDeclarations.CFF2Glyphs;

internal interface IFontDictSelector
{
    IFontDictExecutorSelector GetSelector(Span<IGlyphSubroutineExecutor> indexes);

}

internal interface IFontDictExecutorSelector
{
    IGlyphSubroutineExecutor GetExecutor(uint glyph);
}

[StaticSingleton]
internal partial class SingleDictSelector: IFontDictSelector
{
    public IFontDictExecutorSelector GetSelector(Span<IGlyphSubroutineExecutor> indexes)
    {
        Debug.Assert(indexes.Length == 1);
        return indexes[0];
    }
}

internal sealed class Type0FontDictSelector(byte[] table): RootFontDictSelector
{
    protected override int SelectIndexFor(uint glyph) => table[glyph];
}


internal readonly struct FontDictSelectParser(
    IMultiplexSource source, int offset, uint glyphCount)
{
    public ValueTask<IFontDictSelector> ParseAsync()
    {
        if (offset == 0) return new(SingleDictSelector.Instance);
        return ParseAsync(source.ReadPipeFrom(offset));
    }

    private async ValueTask<IFontDictSelector> ParseAsync(PipeReader pipe)
    {
        var result = await pipe.ReadAsync().CA();
        var type = result.Buffer.First.Span[0];
        pipe.AdvanceTo(result.Buffer.GetPosition(1));
        return type switch
        {
            0 => await ReadType0SelectorAsync(pipe).CA(),
            3 => await new Type34FontSelectorParser(pipe, 2,1).ParseAsync().CA(),
            4 => await new Type34FontSelectorParser(pipe, 4,2).ParseAsync().CA(),
            _ => throw new InvalidDataException("Unknown font dict selector type")
        };
    }

    private async ValueTask<IFontDictSelector> ReadType0SelectorAsync(PipeReader pipe)
    {
        var data = new byte[(int)glyphCount];
        var result = await pipe.ReadAtLeastAsync(data.Length).CA();
        result.Buffer.CopyTo(data);
        pipe.AdvanceTo(result.Buffer.GetPosition(data.Length));
        return new Type0FontDictSelector(data);

    }
}
