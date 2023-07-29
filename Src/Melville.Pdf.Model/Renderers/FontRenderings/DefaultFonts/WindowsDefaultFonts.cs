using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading.Tasks;
using Melville.INPC;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Model.CharacterEncoding;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Objects2;
using Melville.Pdf.LowLevel.Model.Primitives;
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
            var x when x.Equals(KnownNames.CourierTName) => SystemFont(CourierNew,  false, false),
            var x when x.Equals(KnownNames.CourierBoldTName) => SystemFont(CourierNew,  true, false),
            var x when x.Equals(KnownNames.CourierObliqueTName) => SystemFont(CourierNew,  false, true),
            var x when x.Equals(KnownNames.CourierBoldObliqueTName) => SystemFont(CourierNew,  true, true),
            var x when x.Equals(KnownNames.HelveticaTName) => SystemFont(Arial,  false, false),
            var x when x.Equals(KnownNames.HelveticaBoldTName) => SystemFont(Arial,  true, false),
            var x when x.Equals(KnownNames.HelveticaObliqueTName) => SystemFont(Arial,  false, true),
            var x when x.Equals(KnownNames.HelveticaBoldObliqueTName) => SystemFont(Arial,  true, true),
            var x when x.Equals(KnownNames.TimesRomanTName) => SystemFont(TimesNewRoman,  false, false),
            var x when x.Equals(KnownNames.TimesBoldTName) => SystemFont(TimesNewRoman,  true, false),
            var x when x.Equals(KnownNames.TimesObliqueTName) => SystemFont(TimesNewRoman,  false, true),
            var x when x.Equals(KnownNames.TimesBoldObliqueTName) => SystemFont(TimesNewRoman,  true, true),
            var x when x.Equals(KnownNames.SymbolTName) => SystemFont(SegoeUISymbol,  false, false), 
            var x when x.Equals(KnownNames.ZapfDingbatsTName) => SystemFont(SegoeUISymbol,  false, false),
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