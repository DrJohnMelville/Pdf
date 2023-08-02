using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Melville.INPC;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.Model.Documents;
using Melville.Pdf.Model.Renderers.FontRenderings.FontLibraries;
using Melville.Pdf.Model.Renderers.FontRenderings.FreeType;
using Melville.Pdf.Model.Renderers.FontRenderings.FreeType.FontLibraries;
using Melville.Postscript.Interpreter.Values;

namespace Melville.Pdf.Model.Renderers.FontRenderings.DefaultFonts;

/// <summary>
/// This is a class tries to construct named font by checking the windows font directory.
/// </summary>
[StaticSingleton]
public partial class WindowsDefaultFonts : IDefaultFontMapper
{
    private static readonly byte[] TimesNewRoman =
        { 84, 105, 109, 101, 115, 78, 101, 119, 82, 111, 109, 97, 110 };

    private static ReadOnlySpan<byte> CourierNew => "CourierNew"u8;
    private static ReadOnlySpan<byte> Arial => "Arial"u8;
    private static ReadOnlySpan<byte> SegoeUISymbol => "Segoe UI Symbol"u8;

    /// <inheritdoc />
    public DefaultFontReference FontFromName(PdfDirectValue font, FontFlags fontFlags)
    {
        return font switch
        {
            _ when font.Equals(KnownNames.CourierTName) => SystemFont(CourierNew,  false, false),
            _ when font.Equals(KnownNames.CourierBoldTName) => SystemFont(CourierNew,  true, false),
            _ when font.Equals(KnownNames.CourierObliqueTName) => SystemFont(CourierNew,  false, true),
            _ when font.Equals(KnownNames.CourierBoldObliqueTName) => SystemFont(CourierNew,  true, true),
            _ when font.Equals(KnownNames.HelveticaTName) => SystemFont(Arial,  false, false),
            _ when font.Equals(KnownNames.HelveticaBoldTName) => SystemFont(Arial,  true, false),
            _ when font.Equals(KnownNames.HelveticaObliqueTName) => SystemFont(Arial,  false, true),
            _ when font.Equals(KnownNames.HelveticaBoldObliqueTName) => SystemFont(Arial,  true, true),
            _ when font.Equals(KnownNames.TimesRomanTName) => SystemFont(TimesNewRoman,  false, false),
            _ when font.Equals(KnownNames.TimesBoldTName) => SystemFont(TimesNewRoman,  true, false),
            _ when font.Equals(KnownNames.TimesObliqueTName) => SystemFont(TimesNewRoman,  false, true),
            _ when font.Equals(KnownNames.TimesBoldObliqueTName) => SystemFont(TimesNewRoman,  true, true),
            _ when font.Equals(KnownNames.SymbolTName) => SystemFont(SegoeUISymbol,  false, false), 
            _ when font.Equals(KnownNames.ZapfDingbatsTName) => SystemFont(SegoeUISymbol,  false, false),
            _ => TrySystemFont(font, 
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
        PdfDirectValue pdfName, bool bold, bool italic)
    {
        Span<byte> name = pdfName.Get<StringSpanSource>().GetSpan();
        var fontReference = SystemFontLibrary().FontFromName(name, bold, italic);
        return fontReference?.AsDefaultFontReference();
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