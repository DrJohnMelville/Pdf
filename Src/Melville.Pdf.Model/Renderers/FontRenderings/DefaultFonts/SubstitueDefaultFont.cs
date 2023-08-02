using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.Model.Documents;

namespace Melville.Pdf.Model.Renderers.FontRenderings.DefaultFonts;

/// <summary>
/// Given a FontFlags value select a built in font that most closely  replaces the font.
/// This is used when a PDF file is missing a font or the font cannot be read and the
/// reader has to substitute a builtin font.
/// </summary>
public static class SubstituteDefaultFont
{
    private static readonly PdfDirectValue[] names =
    {
        KnownNames.CourierTName,
        KnownNames.CourierBoldTName,
        KnownNames.CourierObliqueTName,
        KnownNames.CourierBoldObliqueTName,
        KnownNames.HelveticaTName,
        KnownNames.HelveticaBoldTName,
        KnownNames.HelveticaObliqueTName,
        KnownNames.HelveticaBoldObliqueTName,
        KnownNames.TimesRomanTName,
        KnownNames.TimesBoldTName,
        KnownNames.TimesObliqueTName,
        KnownNames.TimesBoldObliqueTName,
        KnownNames.SymbolTName,
        KnownNames.SymbolTName,
        KnownNames.SymbolTName,
        KnownNames.SymbolTName,
    };
    
    /// <summary>
    /// Get a default font PdfName that most closely resembles the font flags given
    /// </summary>
    /// <param name="flags">Fontflags for the type to immitate</param>
    /// <returns>A PdfName corresponding to a built in font.</returns>
    public static PdfDirectValue MapBuiltInFont(this FontFlags flags) => 
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