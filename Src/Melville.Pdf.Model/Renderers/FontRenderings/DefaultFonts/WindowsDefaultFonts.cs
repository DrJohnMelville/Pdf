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
using Melville.Pdf.Model.Renderers.FontRenderings.FreeType.FontLibraries;
using Melville.Postscript.Interpreter.Values;
using Microsoft.CodeAnalysis.CSharp.Syntax;

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
    public async ValueTask<DefaultFontReference> FontReferenceForAsync(PdfDictionary dict)
    {
        var font = new PdfFont(dict);
        return await FontFromNameAsync(
            await font.OsFontNameAsync().CA(),
            await font.FontFlagsAsync().CA(), new PdfFont(dict)
            ).CA();
    }

    /// <inheritdoc />
    public ValueTask<DefaultFontReference> FontFromNameAsync(
        PdfDirectObject font, FontFlags flags) =>
        FontFromNameAsync(font, flags, new PdfFont(PdfDictionary.Empty));
    
    private async ValueTask<DefaultFontReference> FontFromNameAsync(
        PdfDirectObject font, FontFlags flags, PdfFont fontStruct)
    {
        var fl = await SystemFontLibraryAsync().CA();
        if (FontFromName(font, flags, fl) is { } f1) return f1;
        var sysinfo = await fontStruct.CidSystemInfoAsync().CA();
        if (sysinfo is not null)
        {
            var ordering = await sysinfo.GetOrDefaultAsync(KnownNames.Ordering, KnownNames.Identity).CA();

            return SystemFont(AsianFontName(ordering, flags.HasFlag(FontFlags.Serif)), false, false, fl);
        }
        return FontFromName(font, flags, fl)??
               FontFromName(GenericFontName(flags, fontStruct), flags, fl)??
               throw new InvalidDataException("Cannot find Built in font.");
    }

    private ReadOnlySpan<byte> AsianFontName(PdfDirectObject ordering, bool serif) => serif switch
    {
        _ when ordering.Equals(KnownNames.GB1) => "Microsoft YaHei"u8,
        true when ordering.Equals(KnownNames.CNS1) => "SimSun"u8,
        false when ordering.Equals(KnownNames.CNS1) => "Microsoft JhengHei"u8,
        true when ordering.Equals(KnownNames.Japan1) => "MS PMincho"u8,
        false when ordering.Equals(KnownNames.Japan1) => "Meiryo"u8,
        true when ordering.Equals(KnownNames.Korea1) => "Batang"u8,
        false when ordering.Equals(KnownNames.Korea1) => "Malgun Gothic"u8,
        true when ordering.Equals(KnownNames.KR) => "Batang"u8,
        false when ordering.Equals(KnownNames.KR) => "Malgun Gothic"u8,
        _ => SegoeUISymbol
    };

    private ReadOnlySpan<byte> GenericFontName(FontFlags flags, PdfFont font)
    {
        return EnglishFontNames(flags);
    }

    private static ReadOnlySpan<byte> EnglishFontNames(FontFlags flags) =>
        flags switch
        {
            _ when flags.HasFlag(FontFlags.Symbolic) => SegoeUISymbol,
            _ when flags.HasFlag(FontFlags.FixedPitch) => CourierNew,
            _ when flags.HasFlag(FontFlags.Serif) => TimesNewRoman,
            _ => Arial
        };


    private DefaultFontReference? FontFromName(
        PdfDirectObject font, FontFlags fontFlags, FontLibrary fl) =>
        font switch
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
                     fontFlags.HasFlag(FontFlags.ForceBold), fontFlags.HasFlag(FontFlags.Italic), fl)
        };

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