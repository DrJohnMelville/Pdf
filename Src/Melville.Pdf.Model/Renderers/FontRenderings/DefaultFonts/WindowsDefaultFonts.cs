using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading.Tasks;
using Melville.INPC;
using Melville.Parsing.AwaitConfiguration;
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
    public async ValueTask<DefaultFontReference> FontFromNameAsync(
        PdfDirectObject font, FontFlags flags)
    {
        return FontFromName(font, flags, await SystemFontLibraryAsync().CA());
    }

    private DefaultFontReference FontFromName(
        PdfDirectObject font, FontFlags fontFlags, FontLibrary fl)
    {
        return font switch
        {
            _ when font.Equals(KnownNames.Courier) => SystemFont(CourierNew,  false, false, fl),
            _ when font.Equals(KnownNames.CourierBold) => SystemFont(CourierNew,  true, false, fl),
            _ when font.Equals(KnownNames.CourierOblique) => SystemFont(CourierNew,  false, true, fl),
            _ when font.Equals(KnownNames.CourierBoldOblique) => SystemFont(CourierNew,  true, true, fl),
            _ when font.Equals(KnownNames.CourierItalic) => SystemFont(CourierNew,  false, true, fl),
            _ when font.Equals(KnownNames.CourierBoldItalic) => SystemFont(CourierNew,  true, true, fl),
            _ when font.Equals(KnownNames.Helvetica) => SystemFont(Arial,  false, false, fl),
            _ when font.Equals(KnownNames.HelveticaBold) => SystemFont(Arial,  true, false, fl),
            _ when font.Equals(KnownNames.HelveticaOblique) => SystemFont(Arial,  false, true, fl),
            _ when font.Equals(KnownNames.HelveticaBoldOblique) => SystemFont(Arial,  true, true, fl),
            _ when font.Equals(KnownNames.HelveticaItalic) => SystemFont(Arial,  false, true, fl),
            _ when font.Equals(KnownNames.HelveticaBoldItalic) => SystemFont(Arial,  true, true, fl),
            _ when font.Equals(KnownNames.TimesRoman) => SystemFont(TimesNewRoman,  false, false, fl),
            _ when font.Equals(KnownNames.TimesBold) => SystemFont(TimesNewRoman,  true, false, fl),
            _ when font.Equals(KnownNames.TimesOblique) => SystemFont(TimesNewRoman,  false, true, fl),
            _ when font.Equals(KnownNames.TimesBoldOblique) => SystemFont(TimesNewRoman,  true, true, fl),
            _ when font.Equals(KnownNames.TimesItalic) => SystemFont(TimesNewRoman,  false, true, fl),
            _ when font.Equals(KnownNames.TimesBoldItalic) => SystemFont(TimesNewRoman,  true, true, fl),
            _ when font.Equals(KnownNames.Symbol) => SystemFont(SegoeUISymbol,  false, false, fl), 
            _ when font.Equals(KnownNames.ZapfDingbats) => SystemFont(SegoeUISymbol,  false, false, fl),
            _ => TrySystemFont(font, 
                        fontFlags.HasFlag(FontFlags.ForceBold), fontFlags.HasFlag(FontFlags.Italic), fl)??
                 FontFromName(fontFlags.MapBuiltInFont(), fontFlags, fl)
        };
    }

    private  DefaultFontReference SystemFont(
        ReadOnlySpan<byte> name, bool bold, bool italic, FontLibrary fl)
    {
        var fontReference = fl.FontFromName(name, bold, italic);
        return fontReference?.AsDefaultFontReference()
               ?? throw new IOException("Could not find required font file.");
    }
    private  DefaultFontReference? TrySystemFont(
        PdfDirectObject pdfName, bool bold, bool italic, FontLibrary fl)
    {
        Span<byte> name = pdfName.Get<StringSpanSource>().GetSpan();
        var fontReference = fl.FontFromName(name, bold, italic);
        return fontReference?.AsDefaultFontReference();
    }


    private static Lazy<Task<FontLibrary>> systemFontLibrary =
        CreateFontSource(Environment.GetFolderPath(Environment.SpecialFolder.Fonts));
    private static Task<FontLibrary> SystemFontLibraryAsync() => systemFontLibrary.Value;

    /// <summary>
    /// Sets the folder from which Melville.Pdf will look for system font files.
    /// </summary>
    /// <param name="fontFolder">The folder to look in for font files.</param>
    [MemberNotNull(nameof(systemFontLibrary))]
    public static void SetFontDirectory(string fontFolder) =>
        systemFontLibrary = CreateFontSource(fontFolder);

    private static Lazy<Task<FontLibrary>> CreateFontSource(string fontFolder)
    {
        return new(() => new FontLibraryBuilder()
            .BuildFromAsync(fontFolder)
            .AsTask());
    }
}