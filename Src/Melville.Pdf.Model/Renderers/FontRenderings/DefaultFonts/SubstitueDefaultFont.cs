using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.Model.Documents;

namespace Melville.Pdf.Model.Renderers.FontRenderings.DefaultFonts;

public static class SubstituteDefaultFont
{
    private static readonly PdfName[] names =
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
    
    public static PdfName MapBuiltInFont(this FontFlags flags) => 
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