using System;
using System.IO;
using System.Reflection.Metadata;
using System.Threading.Tasks;
using Melville.INPC;
using Melville.Parsing.AwaitConfiguration;
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
    /// Create a DefaultFontReference for a given font dictionary.
    /// </summary>
    /// <param name="dict"></param>
    /// <returns></returns>
    async ValueTask<DefaultFontReference> FontReferenceForAsync(PdfDictionary dict)
    {
        var font = new PdfFont(dict);
        return await FontFromNameAsync(
            await font.OsFontNameAsync().CA(),
            await font.FontFlagsAsync().CA()).CA();
    }

    /// <summary>
    /// Get a Default Font reference for a given PDF Font name.
    /// </summary>
    /// <param name="font">The PDFName of the font</param>
    /// <param name="flags">The fontflags from the font structure</param>
    /// <returns>A DefaultFontReference from which the font can be built.</returns>
    [Obsolete("Use the FontReferenceForAsync")]
    ValueTask<DefaultFontReference> FontFromNameAsync(PdfDirectObject font, FontFlags flags);
}
