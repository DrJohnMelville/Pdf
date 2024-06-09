using System.Buffers;
using System.Numerics;
using Melville.Parsing.AwaitConfiguration;

namespace Melville.Fonts.SfntParsers.TableDeclarations.CffGlyphs;

/// <summary>
/// This is a source of CFF glyphs.
/// </summary>
public class CffGlyphSource : IGlyphSource
{
    private readonly CffIndex glyphs;
    private readonly IGlyphSubroutineExecutor globalSubrs;
    private readonly IGlyphSubroutineExecutor localSubrs;
    private readonly Matrix3x2 glyphUnitAdjuster;

    internal CffGlyphSource(
        CffIndex glyphs, 
        IGlyphSubroutineExecutor globalSubrs, 
        IGlyphSubroutineExecutor localSubrs,
        uint unitsPerEm)
    {
        this.glyphs = glyphs;
        this.globalSubrs = globalSubrs;
        this.localSubrs = localSubrs;
        glyphUnitAdjuster = Matrix3x2.CreateScale(1f/unitsPerEm);
    }

    /// <inheritdoc />
    public int GlyphCount => (int)glyphs.Length;

    /// <summary>
    /// Render a glyph to the target.
    /// </summary>
    /// <param name="glyph">The glyph number</param>
    /// <param name="target">The target to draw to</param>
    /// <param name="transform">The transform matrix for the glyph rendering</param>
    public async ValueTask RenderGlyph(uint glyph, ICffGlyphTarget target, Matrix3x2 transform)
    {
        if (glyph > GlyphCount) glyph = 0;
        var sourceSequence = await glyphs.ItemDataAsync((int)glyph).CA();
        using var engine = new CffInstructionExecutor(
            target, glyphUnitAdjuster*transform, globalSubrs, localSubrs);

        await engine.ExecuteInstructionSequenceAsync(sourceSequence).CA();
   }
}
