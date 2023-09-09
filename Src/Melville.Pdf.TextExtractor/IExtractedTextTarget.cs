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
}