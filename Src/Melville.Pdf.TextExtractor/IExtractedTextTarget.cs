using System.Numerics;
using Melville.Pdf.Model.Renderers.FontRenderings;

namespace Melville.Pdf.TextExtractor;

/// <summary>
/// This is a target for text extracted from a PDF file.
/// </summary>
public interface IExtractedTextTarget
{
    /// <summary>
    /// Begin a single PDF text writing operation.
    /// </summary>
    /// <param name="font">The font being written in.</param>
    void BeginWrite(IRealizedFont font);
    /// <summary>
    /// End a font writing operation.
    /// </summary>
    /// <param name="textMatrix">The text matrix at the end of the font write operation.</param>
    void EndWrite(in Matrix3x2 textMatrix);
    /// <summary>
    /// Called for each character written to a PDF file.
    /// </summary>
    /// <param name="character">The character to write.</param>
    /// <param name="textMatrix">The text matrix at the time of the write operation</param>
    void WriteCharacter(char character, in Matrix3x2 textMatrix);

    /// <summary>
    /// This notifies the writer of an x offset inside og a Td operation.  The writer does not
    /// need to update the current point, but is just notified that the jump has happened.
    /// (The text extractor uses this to add spaces to strings)
    /// </summary>
    /// <param name="value">The delta value out of the source pdf stream, positive is to the left</param>
    void DeltaInsideWrite(double value);

}