using System.Buffers;
using Melville.Fonts.SfntParsers.TableDeclarations.CffGlyphs;
using Melville.INPC;

namespace Melville.Fonts.Type1TextParsers;

internal sealed partial class Type1SubrExecutor:IGlyphSubroutineExecutor
{
    [FromConstructor] private readonly Memory<byte>[] subRoutines;

    public IGlyphSubroutineExecutor GetExecutor(uint glyph) => this;

    public ValueTask CallAsync(int subroutine, Func<ReadOnlySequence<byte>, ValueTask> execute) => 
        execute(new ReadOnlySequence<byte>(subRoutines[subroutine]));
}