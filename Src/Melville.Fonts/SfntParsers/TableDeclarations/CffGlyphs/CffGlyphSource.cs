using System.Buffers;
using System.Numerics;
using Melville.Fonts.SfntParsers.TableDeclarations.CFF2Glyphs;
using Melville.Parsing.AwaitConfiguration;

namespace Melville.Fonts.SfntParsers.TableDeclarations.CffGlyphs;

/// <summary>
/// This is a source of CFF glyphs.
/// </summary>
public class CffGlyphSource : IGlyphSource
{
    private readonly CffIndex glyphs;
    private readonly IFontDictExecutorSelector globalSubrs;
    private readonly IFontDictExecutorSelector localSubrs;
    private readonly Matrix3x2 glyphUnitAdjuster;

    internal CffGlyphSource(
        CffIndex glyphs, 
        IFontDictExecutorSelector globalSubrs, 
        IFontDictExecutorSelector localSubrs,
        in Matrix3x2 glyphUnitAdjustment)
    {
        this.glyphs = glyphs;
        this.globalSubrs = globalSubrs;
        this.localSubrs = localSubrs;
        glyphUnitAdjuster = glyphUnitAdjustment;
    }

    /// <inheritdoc />
    public int GlyphCount => (int)glyphs.Length;

    /// <summary>
    /// Render a glyph to the target.
    /// </summary>
    /// <param name="glyph">The glyph number</param>
    /// <param name="target">The target to draw to</param>
    /// <param name="transform">The transform matrix for the glyph rendering</param>
    public async ValueTask RenderGlyphAsync(uint glyph, ICffGlyphTarget target, Matrix3x2 transform)
    {
        if (glyph > GlyphCount) glyph = 0;
        var sourceSequence = await glyphs.ItemDataAsync((int)glyph).CA();
        using var engine = new CffInstructionExecutor(
            target, glyphUnitAdjuster*transform, globalSubrs.GetExecutor(glyph), localSubrs.GetExecutor(glyph));

        await engine.ExecuteInstructionSequenceAsync(sourceSequence).CA();
   }
}
