using System.IO;
using Melville.INPC;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.Model.Documents;

namespace Melville.Pdf.Model.Renderers.FontRenderings.DefaultFonts;

/// <summary>
/// References a specific font, by index within a font file.
/// </summary>
public readonly partial struct DefaultFontReference
{
    /// <summary>
    /// The stream from which the font can be read.
    /// </summary>
    [FromConstructor] public Stream Source { get; }
    /// <summary>
    /// The index of the font within the stream.
    /// </summary>
    [FromConstructor] public int Index { get; }
}

/// <summary>
/// A strategy implementation for mapping PDF default font names to actual fonts.
/// </summary>
public interface IDefaultFontMapper
{
    /// <summary>
    /// Get a Default Font reference for a given PDF Font name.
    /// </summary>
    /// <param name="font">The PDFName of the font</param>
    /// <param name="flags">The fontflags from the font structure</param>
    /// <returns>A DefaultFontReference from which the font can be built.</returns>
    DefaultFontReference FontFromName(PdfDirectValue font, FontFlags flags);
}
