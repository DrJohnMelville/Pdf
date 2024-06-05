using System.Buffers;
using Melville.Parsing.AwaitConfiguration;

namespace Melville.Fonts.SfntParsers.TableDeclarations.CffGlyphs;

public interface ICffGlyphTarget
{
    void Instruction(int instruction, ReadOnlySpan<DictValue> values);
}

/// <summary>
/// This is a source of CFF glyphs.
/// </summary>
public class CffGlyphSource : IGlyphSource
{
    private readonly CffIndex glyphs;
    internal CffGlyphSource(CffIndex glyphs) => this.glyphs = glyphs;

    /// <inheritdoc />
    public int GlyphCount => (int)glyphs.Length;

    public async ValueTask RenderGlyph(uint glyph, ICffGlyphTarget target)
    {
        if (glyph > GlyphCount) glyph = 0;
        ExecuteGlyph((await glyphs.ItemDataAsync((int)glyph).CA()), target);
    }

    // per Adobe Technical Note #5176, page 11
    private const int MaximumCffInstructionOperands = 48;
    private void ExecuteGlyph(ReadOnlySequence<byte> itemDataAsync, ICffGlyphTarget target)
    {
        var stack = ArrayPool<DictValue>.Shared.Rent(MaximumCffInstructionOperands);
        var parser = new DictParser<CharString2Definition>(new SequenceReader<byte>(itemDataAsync), stack);

        while (parser.ReadNextInstruction() is var op and not 255)
        {
            target.Instruction(op, parser.Operands);
        }

        ArrayPool<DictValue>.Shared.Return(stack);
    }
}