using System.Numerics;

namespace Melville.Fonts.SfntParsers.TableDeclarations.CffGlyphs;

/// <summary>
/// This interface is the target to which a CFF glyph is rendered.
/// </summary>
public interface ICffGlyphTarget: IGlyphTarget
{
    /// <summary>
    /// Calling this method is optional and the production parsers only call it in
    /// debug builds.  This function receives reports of the CFF opcodes
    /// and stack state that will be executed by the CharString interpreter.
    /// The font viewers in the low level viewer use this functionality to
    /// display the CFF instructions that are about to be executed.
    /// </summary>
    /// <param name="opCode">The opcode that is about to be executed</param>
    /// <param name="stack">The operand stack at the time of the call</param>
    void Operator(CharStringOperators opCode, Span<DictValue> stack);

    /// <summary>
    /// Report the difference between the default glyph width and this glyph's width.
    /// </summary>
    /// <param name="delta">Difference thisGlyphWidth - DefaultGlyphWidth</param>
    void RelativeCharWidth(float delta);
}