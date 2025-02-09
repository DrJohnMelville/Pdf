using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.Model.Documents;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Melville.Pdf.Model.Renderers.FontRenderings.DefaultFonts;

/// <summary>
/// Given a FontFlags value select a built in font that most closely  replaces the font.
/// This is used when a PDF file is missing a font or the font cannot be read and the
/// reader has to substitute a builtin font.
/// </summary>
public static class SubstituteDefaultFont
{
    private static readonly PdfDirectObject[] names =
    {
        KnownNames.Courier,
        KnownNames.CourierBold,
        KnownNames.CourierOblique,
        KnownNames.CourierBoldOblique,
        KnownNames.Helvetica,
        KnownNames.HelveticaBold,
        KnownNames.HelveticaOblique,
        KnownNames.HelveticaBoldOblique,
        KnownNames.TimesRoman,
        KnownNames.TimesBold,
        KnownNames.TimesOblique,
        KnownNames.TimesBoldOblique,
        KnownNames.Symbol,
        KnownNames.Symbol,
        KnownNames.Symbol,
        KnownNames.Symbol,
    };
    
    /// <summary>
    /// Get a default font PdfName that most closely resembles the font flags given
    /// </summary>
    /// <param name="flags">Fontflags for the type to immitate</param>
    /// <returns>A PdfName corresponding to a built in font.</returns>
    public static PdfDirectObject MapBuiltInFont(this FontFlags flags) => 
      names[FamilyOffset(flags) + BoldOffset(flags) + ItalicOffset(flags)];

    private static int FamilyOffset(FontFlags flags)
    {
        if (flags.HasFlag(FontFlags.Symbolic)) return 12;
        if (flags.HasFlag(FontFlags.FixedPitch)) return 0;
        if (flags.HasFlag(FontFlags.Serif)) return 8;
        return 4;

    }

    private static int BoldOffset(FontFlags flags) => flags.HasFlag(FontFlags.ForceBold) ? 1 : 0;
    private static int ItalicOffset(FontFlags flags) => flags.HasFlag(FontFlags.Italic) ? 2 : 0;
}