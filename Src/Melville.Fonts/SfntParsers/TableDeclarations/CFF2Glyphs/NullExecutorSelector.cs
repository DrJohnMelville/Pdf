using System.Buffers;
using Melville.Fonts.SfntParsers.TableDeclarations.CffGlyphs;
using Melville.INPC;

namespace Melville.Fonts.SfntParsers.TableDeclarations.CFF2Glyphs;

[StaticSingleton]
internal partial class NullExecutorSelector : 
    IFontDictExecutorSelector, IGlyphSubroutineExecutor
{
    public IGlyphSubroutineExecutor GetExecutor(uint glyph) => this;
    public ValueTask CallAsync(
        int subroutine, ICffInstructionExecutor execute)
    {
        throw new InvalidOperationException("Executed subr from an empty index.");
    }
}