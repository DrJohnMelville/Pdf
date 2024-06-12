using System.Numerics;

namespace Melville.Fonts.SfntParsers.TableDeclarations.CffGlyphs
{
    /// <summary>
    /// This interface is the target to which a CFF glyph is rendered.
    /// </summary>
    public interface ICffGlyphTarget
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
    
        /// <summary>
        /// Begin a new contour the given location.
        /// </summary>
        /// <param name="point">A scaled point to move the current position</param>
        void MoveTo(Vector2 point);
    
        /// <summary>
        /// Draw a straight line from the previous location to this location and set
        /// the current location to this location.
        /// </summary>
        /// <param name="point">The endpoint of the line segment to draw</param>
        void LineTo(Vector2 point);

        /// <summary>
        /// Draws a Bezier curve from the current location to the endpoint with the
        /// given control points.
        /// </summary>
        /// <param name="control1">First control point</param>
        /// <param name="control2">Second control point</param>
        /// <param name="endPoint">End point</param>
        void CurveTo(Vector2 control1, Vector2 control2, Vector2 endPoint);

        /// <summary>
        /// Indicates that no more instructions that will be for this character
        /// </summary>
        void EndGlyph();
    }
}