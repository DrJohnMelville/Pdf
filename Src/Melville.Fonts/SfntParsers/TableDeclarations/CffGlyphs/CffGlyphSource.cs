using System.Buffers;
using System.Numerics;
using Melville.Fonts.SfntParsers.TableDeclarations.CFF2Glyphs;
using Melville.INPC;
using Melville.Parsing.AwaitConfiguration;

namespace Melville.Fonts.SfntParsers.TableDeclarations.CffGlyphs;

internal readonly partial struct CffGlyphTargetWrapper<T>:ICffGlyphTarget where T: IGlyphTarget
{
    [FromConstructor] [DelegateTo] private readonly IGlyphTarget target;
    public void Operator(CharStringOperators opCode, Span<DictValue> stack)
    {
    }

    public void RelativeCharWidth(float delta)
    {
    }
}

/// <summary>
/// This is a source of CFF glyphs.
/// </summary>
public class CffGlyphSource : IGlyphSource
{
    private readonly CffIndex glyphs;
    private readonly IFontDictExecutorSelector globalSubrs;
    private readonly IFontDictExecutorSelector localSubrs;
    private readonly Matrix3x2 glyphUnitAdjuster;
    private readonly uint[] variatons;
 
    internal CffGlyphSource(CffIndex glyphs,
        IFontDictExecutorSelector globalSubrs,
        IFontDictExecutorSelector localSubrs,
        in Matrix3x2 glyphUnitAdjustment, uint[] variatons)
    {
        this.glyphs = glyphs;
        this.globalSubrs = globalSubrs;
        this.localSubrs = localSubrs;
        glyphUnitAdjuster = glyphUnitAdjustment;
        this.variatons = variatons;
    }

    /// <inheritdoc />
    public int GlyphCount => (int)glyphs.Length;

    /// <inheritdoc />
    public ValueTask RenderGlyphAsync<T>(uint glyph, T target, Matrix3x2 transform) where T : IGlyphTarget => 
        RenderCffGlyphAsync<CffGlyphTargetWrapper<T>>(glyph, new(target), transform);

    /// <summary>
    /// Render a glyph to the target.
    /// </summary>
    /// <param name="glyph">The glyph number</param>
    /// <param name="target">The target to draw to</param>
    /// <param name="transform">The transform matrix for the glyph rendering</param>
    public async ValueTask RenderCffGlyphAsync<T>(uint glyph, T target, Matrix3x2 transform) where T: ICffGlyphTarget
    {
        if (glyph >= GlyphCount) glyph = 0;
        var sourceSequence = await glyphs.ItemDataAsync((int)glyph).CA();
        using var engine = new CffInstructionExecutor<T>(
            target, glyphUnitAdjuster*transform, globalSubrs.GetExecutor(glyph), 
            localSubrs.GetExecutor(glyph), variatons);

        await engine.ExecuteInstructionSequenceAsync(sourceSequence).CA();
   }
}
