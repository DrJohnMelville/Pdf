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
    IFontDictExecutorSelector GetSelector(Span<CffIndex> indexes);

}

internal interface IFontDictExecutorSelector
{
    IGlyphSubroutineExecutor GetExecutor(uint glyph);
}

[StaticSingleton]
internal partial class EmptySelector: IFontDictSelector
{
    public IFontDictExecutorSelector GetSelector(Span<CffIndex> indexes)
    {
        Debug.Assert(indexes.Length == 1);
        return new GlyphSubroutineExecutor(indexes[0]);
    }
}

[StaticSingleton]
internal partial class NullExecutorSelector : 
    IFontDictExecutorSelector, IGlyphSubroutineExecutor
{
     public IGlyphSubroutineExecutor GetExecutor(uint glyph) => this;
     public ValueTask CallAsync(
         int subroutine, Func<ReadOnlySequence<byte>, ValueTask> execute)
     {
         throw new InvalidOperationException("Executed subr from an empty index.");
     }
}

internal readonly struct FontDictSelectParser(
    IMultiplexSource source, int offset, uint glyphCount)
{
    public ValueTask<IFontDictSelector> ParseAsync()
    {
        if (offset == 0) return new(EmptySelector.Instance);
        return ParseAsync(source.ReadPipeFrom(offset));
    }

    private async ValueTask<IFontDictSelector> ParseAsync(PipeReader pipe)
    {
        var result = await pipe.ReadAsync().CA();
        var type = result.Buffer.First.Span[0];
        pipe.AdvanceTo(result.Buffer.GetPosition(1));
        throw new NotImplementedException();
    }
}