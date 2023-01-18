using System;
using System.IO;
using System.Threading.Tasks;
using Melville.INPC;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Model.CharacterEncoding;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.Model.Documents;
using Melville.Pdf.Model.Renderers.FontRenderings.FontLibraries;
using Melville.Pdf.Model.Renderers.FontRenderings.FreeType;
using Melville.Pdf.Model.Renderers.FontRenderings.FreeType.FontLibraries;

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

    public DefaultFontReference FontFromName(
        PdfName font, FontFlags fontFlags)
    {
        return font.GetHashCode() switch
        {
            KnownNameKeys.Courier => SystemFont(CourierNew,  false, false),
            KnownNameKeys.CourierBold => SystemFont(CourierNew,  true, false),
            KnownNameKeys.CourierOblique => SystemFont(CourierNew,  false, true),
            KnownNameKeys.CourierBoldOblique => SystemFont(CourierNew,  true, true),
            KnownNameKeys.Helvetica => SystemFont(Arial,  false, false),
            KnownNameKeys.HelveticaBold => SystemFont(Arial,  true, false),
            KnownNameKeys.HelveticaOblique => SystemFont(Arial,  false, true),
            KnownNameKeys.HelveticaBoldOblique => SystemFont(Arial,  true, true),
            KnownNameKeys.TimesRoman => SystemFont(TimesNewRoman,  false, false),
            KnownNameKeys.TimesBold => SystemFont(TimesNewRoman,  true, false),
            KnownNameKeys.TimesOblique => SystemFont(TimesNewRoman,  false, true),
            KnownNameKeys.TimesBoldOblique => SystemFont(TimesNewRoman,  true, true),
            KnownNameKeys.Symbol => SystemFont(SegoeUISymbol,  false, false), 
            KnownNameKeys.ZapfDingbats => SystemFont(SegoeUISymbol,  false, false),
            _ => TrySystemFont(font.Bytes, 
                        fontFlags.HasFlag(FontFlags.ForceBold), fontFlags.HasFlag(FontFlags.Italic))??
                 FontFromName(fontFlags.MapBuiltInFont(), fontFlags)
        };
    }

    private  DefaultFontReference SystemFont(
        byte[] name, bool bold, bool italic)
    {
        var fontReference = SystemFontLibrary().FontFromName(name, bold, italic);
        return fontReference?.AsDefaultFontReference()
               ?? throw new IOException("Could not find required font file.");
    }
    private  DefaultFontReference? TrySystemFont(
        byte[] name, bool bold, bool italic)
    {
        var fontReference = SystemFontLibrary().FontFromName(name, bold, italic);
        if (fontReference is null) return null;
        return fontReference.AsDefaultFontReference();
    }


    private static FontLibrary? systemFontLibrary;
    private static FontLibrary SystemFontLibrary() => 
        systemFontLibrary ?? SetFontDirectory(Environment.GetFolderPath(Environment.SpecialFolder.Fonts));

    public static FontLibrary SetFontDirectory(string fontFolder)
    {
        GlobalFreeTypeMutex.WaitFor();
        try
        {
            return systemFontLibrary =
                new FontLibraryBuilder().BuildFrom(fontFolder);
        }
        finally
        {
            GlobalFreeTypeMutex.Release();
        }
    }
}