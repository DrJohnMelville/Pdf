using System.Buffers;
using Melville.Fonts.SfntParsers.TableDeclarations.CFF2Glyphs;
using Melville.INPC;
using Melville.Parsing.AwaitConfiguration;

namespace Melville.Fonts.SfntParsers.TableDeclarations.CffGlyphs;

internal interface IGlyphSubroutineExecutor: IFontDictExecutorSelector
{
    ValueTask CallAsync(int subroutine, ICffInstructionExecutor executor);
}

internal partial class GlyphSubroutineExecutor : IGlyphSubroutineExecutor
{
    [FromConstructor] private readonly CffIndex subroutines;

    public async ValueTask CallAsync(
        int subroutine, ICffInstructionExecutor executor)
    {
        var index = subroutine+Bias();
        if ((uint)index >= subroutines.Length)
            return; // silently ignore calls to nonexistant subroutines
        using var data = await subroutines.ItemDataAsync(index).CA();
        await executor.ExecuteInstructionSequenceAsync(data.Sequence).CA();
    }

    private int Bias() => subroutines.Length switch
    {
        < 1240 => 107,
        < 33900 => 1131,
        _ => 32768
    };

    public IGlyphSubroutineExecutor GetExecutor(uint glyph) => this;
}