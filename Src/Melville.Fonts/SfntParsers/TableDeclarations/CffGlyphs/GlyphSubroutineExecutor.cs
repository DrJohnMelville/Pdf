using System.Buffers;
using Melville.Fonts.SfntParsers.TableDeclarations.CFF2Glyphs;
using Melville.INPC;
using Melville.Parsing.AwaitConfiguration;

namespace Melville.Fonts.SfntParsers.TableDeclarations.CffGlyphs;

internal interface IGlyphSubroutineExecutor: IFontDictExecutorSelector
{
    ValueTask CallAsync(int subroutine, Func<ReadOnlySequence<byte>, ValueTask> execute);
}

internal partial class GlyphSubroutineExecutor : IGlyphSubroutineExecutor
{
    [FromConstructor] private readonly CffIndex subroutines;

    public async ValueTask CallAsync(
        int subroutine, Func<ReadOnlySequence<byte>, ValueTask> execute)
    {
        var data = await subroutines.ItemDataAsync(subroutine+Bias()).CA();
        await execute(data).CA();
    }

    private int Bias() => subroutines.Length switch
    {
        < 1240 => 107,
        < 33900 => 1131,
        _ => 32768
    };

    public IGlyphSubroutineExecutor GetExecutor(uint glyph) => this;
}