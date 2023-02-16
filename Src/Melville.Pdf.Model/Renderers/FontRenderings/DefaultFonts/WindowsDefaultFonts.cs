using System;
using System.Diagnostics.CodeAnalysis;
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

    private static ReadOnlySpan<byte> CourierNew => "CourierNew"u8;
    private static ReadOnlySpan<byte> Arial => "Arial"u8;
    private static ReadOnlySpan<byte> SegoeUISymbol => "Segoe UI Symbol"u8;

    /// <inheritdoc />
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
        ReadOnlySpan<byte> name, bool bold, bool italic)
    {
        var fontReference = SystemFontLibrary().FontFromName(name, bold, italic);
        return fontReference?.AsDefaultFontReference()
               ?? throw new IOException("Could not find required font file.");
    }
    private  DefaultFontReference? TrySystemFont(
        ReadOnlySpan<byte> name, bool bold, bool italic)
    {
        var fontReference = SystemFontLibrary().FontFromName(name, bold, italic);
        if (fontReference is null) return null;
        return fontReference.AsDefaultFontReference();
    }


    private static FontLibrary? systemFontLibrary;

    private static FontLibrary SystemFontLibrary()
    {
        if (systemFontLibrary is null)
        {
            SetFontDirectory(Environment.GetFolderPath(Environment.SpecialFolder.Fonts));
        }

        return systemFontLibrary;
    }

    /// <summary>
    /// Sets the folder from which Melville.Pdf will look for system font files.
    /// </summary>
    /// <param name="fontFolder">The folder to look in for font files.</param>
    [MemberNotNull(nameof(systemFontLibrary))]
    public static void SetFontDirectory(string fontFolder)
    {
        GlobalFreeTypeMutex.WaitFor();
        try
        {
            systemFontLibrary =
                new FontLibraryBuilder().BuildFrom(fontFolder);
        }
        finally
        {
            GlobalFreeTypeMutex.Release();
        }
    }
}