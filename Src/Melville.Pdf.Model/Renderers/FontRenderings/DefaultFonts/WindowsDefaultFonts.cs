using System.IO;
using System.Threading.Tasks;
using Melville.INPC;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Model.CharacterEncoding;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.Model.Documents;
using Melville.Pdf.Model.Renderers.FontRenderings.FreeType;

namespace Melville.Pdf.Model.Renderers.FontRenderings.DefaultFonts;

[StaticSingleton]
public partial class WindowsDefaultFonts : IDefaultFontMapper
{
    private static readonly byte[] TimesNewRoman =
        { 84, 105, 109, 101, 115, 78, 101, 119, 82, 111, 109, 97, 110 };

    private static readonly byte[] CourierNew =
        { 67, 111, 117, 114, 105, 101, 114, 78, 101, 119 };

    private static readonly byte[] Arial = { 65, 114, 105, 97, 108 };
    private static readonly byte[] SegoeUISymbol =
        { 83, 101, 103, 111, 101, 32, 85, 73, 32, 83, 121, 109, 98, 111, 108 };

    public ValueTask<IRealizedFont>  MapDefaultFont(PdfName font, FontFlags fontFlags, FreeTypeFontFactory factory)
    {
        return font.GetHashCode() switch
        {
            KnownNameKeys.Courier => SystemFont(CourierNew, fontFlags, factory,  false, false),
            KnownNameKeys.CourierBold => SystemFont(CourierNew, fontFlags, factory,  true, false),
            KnownNameKeys.CourierOblique => SystemFont(CourierNew, fontFlags, factory,  false, true),
            KnownNameKeys.CourierBoldOblique => SystemFont(CourierNew, fontFlags, factory,  true, true),
            KnownNameKeys.Helvetica => SystemFont(Arial, fontFlags, factory,  false, false),
            KnownNameKeys.HelveticaBold => SystemFont(Arial, fontFlags, factory,  true, false),
            KnownNameKeys.HelveticaOblique => SystemFont(Arial, fontFlags, factory,  false, true),
            KnownNameKeys.HelveticaBoldOblique => SystemFont(Arial, fontFlags, factory,  true, true),
            KnownNameKeys.TimesRoman => SystemFont(TimesNewRoman, fontFlags, factory,  false, false),
            KnownNameKeys.TimesBold => SystemFont(TimesNewRoman, fontFlags, factory,  true, false),
            KnownNameKeys.TimesOblique => SystemFont(TimesNewRoman, fontFlags, factory,  false, true),
            KnownNameKeys.TimesBoldOblique => SystemFont(TimesNewRoman, fontFlags, factory,  true, true),
            KnownNameKeys.Symbol => SystemFont(SegoeUISymbol, fontFlags, factory,  false, false), 
            KnownNameKeys.ZapfDingbats => SystemFont(SegoeUISymbol, fontFlags, factory,  false, false),
            _ => SystemFont(font.Bytes, fontFlags, factory, 
                        fontFlags.HasFlag(FontFlags.ForceBold), fontFlags.HasFlag(FontFlags.Italic))
        };
    }

    private async ValueTask<IRealizedFont> SystemFont(
        byte[] name, FontFlags flags, FreeTypeFontFactory factory, bool bold, bool italic)
    {
        var fontReference = GlobalFreeTypeResources.SystemFontLibrary().FontFromName(name, bold, italic)??
                            GlobalFreeTypeResources.SystemFontLibrary().FontFromName(FontNameFromFlags(flags), bold, italic)??
                            throw new PdfParseException("Could not replace font.");
        await using var stream = File.Open(fontReference.FileName, FileMode.Open, FileAccess.Read, FileShare.Read);
        return await factory.FromCSharpStream(stream, fontReference.Index).CA();
    }

    private byte[] FontNameFromFlags(FontFlags fontFlags)
    {
        if (fontFlags.HasFlag(FontFlags.Symbolic)) return SegoeUISymbol;
        if (fontFlags.HasFlag(FontFlags.FixedPitch)) return CourierNew;
        if (fontFlags.HasFlag(FontFlags.Serif)) return TimesNewRoman;
        return Arial;
    }
}