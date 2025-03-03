using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Melville.INPC;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.Model.Renderers.FontRenderings.DefaultFonts;
using Melville.Postscript.Interpreter.Values;

namespace Melville.Pdf.Model.Documents;

/// <summary>
/// This is a costume type that represents a PDF font dictionaru
/// </summary>
public record struct PdfFont(PdfDictionary LowLevel)
{
    /// <summary>
    /// Gets the subtype of the font
    /// </summary>
    /// <returns>The subtype object, or Type1 if there is not subtype</returns>
    public PdfDirectObject SubType() => LowLevel.SubTypeOrNull(KnownNames.Type1);
    
    /// <summary>
    /// The encoding object for the font
    /// </summary>
    public async ValueTask<PdfEncoding> EncodingAsync() => 
        new(await LowLevel.GetOrNullAsync(KnownNames.Encoding).CA());

    /// <summary>
    /// The font descriptor for the font
    /// </summary>
    private ValueTask<PdfDictionary?> DescriptorAsync() =>
        LowLevel.GetOrNullAsync<PdfDictionary>(KnownNames.FontDescriptor);

    /// <summary>
    /// The font flags for the font
    /// </summary>
    public async ValueTask<FontFlags> FontFlagsAsync() =>
        await DescriptorAsync().CA() is { } descriptor
            ? (FontFlags)(await descriptor.GetOrDefaultAsync(KnownNames.Flags, 0).CA())
            : FontFlags.None;

    /// <summary>
    /// The PdfStream that holds the embedded font program
    /// </summary>
    public async ValueTask<PdfStream?> EmbeddedStreamAsync() =>
        await DescriptorAsync().CA() is { } descriptor
            && (
            descriptor.TryGetValue(KnownNames.FontFile, out var retTask) ||
                descriptor.TryGetValue(KnownNames.FontFile2, out retTask) ||
              descriptor.TryGetValue(KnownNames.FontFile3, out retTask) )? 
                (await retTask.CA()).Get<PdfStream>(): null;

    /// <summary>
    /// Returns the name of the base font
    /// </summary>
    public ValueTask<PdfDirectObject> BaseFontNameAsync() =>
        LowLevel.GetOrDefaultAsync(KnownNames.BaseFont, KnownNames.Helvetica);

    /// <summary>
    /// Returns the First character of a Type 1 font.
    /// </summary>
    public async ValueTask<byte> FirstCharAsync() =>
        (byte)await LowLevel.GetOrDefaultAsync(KnownNames.FirstChar, 0).CA();

    /// <summary>
    /// Returns the Widths array of a font
    /// </summary>
    /// <returns></returns>
    public async Task<IReadOnlyList<double>?> WidthsArrayAsync() =>
        (await LowLevel.GetOrNullAsync<PdfArray>(KnownNames.Widths).CA()) is { } widthArray
            ? await widthArray.CastAsync<double>().CA()
            : null;
    
    /// <summary>
    /// Return the operating system name for a font
    /// </summary>
    public async ValueTask<PdfDirectObject> OsFontNameAsync() =>
        ComputeOsFontName(SubType(), await BaseFontNameAsync().CA());
    
    private PdfDirectObject ComputeOsFontName(PdfDirectObject fontType, PdfDirectObject baseFontName) =>
        fontType.Equals(KnownNames.MMType1)?
            RemoveMultMasterSuffix(baseFontName):baseFontName;
    
    private PdfDirectObject RemoveMultMasterSuffix(PdfDirectObject baseFontName)
    {
        Span<byte> source = baseFontName.Get<StringSpanSource>().GetSpan();
        var firstUnderscore = source.IndexOf((byte)'_');
        return firstUnderscore < 0 ? baseFontName : PdfDirectObject.CreateName(source[..firstUnderscore]);
    }

    /// <summary>
    /// Returns the subfont of a Type0 font.
    /// </summary>
    public async ValueTask<PdfFont> Type0SubFontAsync() =>
        new PdfFont(
            await (await LowLevel.GetAsync<PdfArray>(KnownNames.DescendantFonts).CA())
                .GetAsync<PdfDictionary>(0).CA()
        );

    /// <summary>
    /// Return a CID system info for the font.
    /// </summary>
    public ValueTask<PdfDictionary?> CidSystemInfoAsync() =>
        LowLevel.GetOrDefaultAsync(KnownNames.CIDSystemInfo, (PdfDictionary?)null);

    /// <summary>
    /// Returns the CIDtoGIDMapStream of a type 0 font
    /// </summary>
    /// <returns></returns>
    public ValueTask<PdfStream?> CidToGidMapStreamAsync() => 
        LowLevel.GetOrDefaultAsync(KnownNames.CIDToGIDMap, (PdfStream?)null);

    /// <summary>
    /// Gets the default width of a font.
    /// </summary>
    /// <returns></returns>
    public ValueTask<double> DefaultWidthAsync() =>
        LowLevel.GetOrDefaultAsync(KnownNames.DW, 1000.0);

    /// <summary>
    /// Gets the Widths array (W) for a CID font
    /// </summary>
    /// <returns></returns>
    public ValueTask<PdfArray> WArrayAsync() =>
        LowLevel.GetOrDefaultAsync(KnownNames.W, PdfArray.Empty);


    /// <summary>
    /// The asian language that a font is defined upon
    /// </summary>
    /// <returns>An enumeration for  the asian language</returns>
    public async ValueTask<AsianLanguages?> FontAsianLanguageAsync()
    {
        return await CidSystemInfoAsync().CA() is { } sysInfo &&
               await sysInfo.GetOrDefaultAsync(KnownNames.Ordering, KnownNames.Identity).CA() is { } order
            ? MapLanguage(order)
            : null;
    }

    private static AsianLanguages? MapLanguage(PdfDirectObject order) =>
        order switch
        {
            _ when order.Equals(KnownNames.GB1) => AsianLanguages.SimplifiedChinese,
            _ when order.Equals(KnownNames.CNS1) => AsianLanguages.TraditionalChinese,
            _ when order.Equals(KnownNames.Japan1) => AsianLanguages.Japanese,
            _ when order.Equals(KnownNames.Korea1) => AsianLanguages.Korean,
            _ when order.Equals(KnownNames.KR) => AsianLanguages.Korean,
            _ => null
        };

    /// <summary>
    /// Gets the ToUnicode mapping entry
    /// </summary>
    public ValueTask<PdfDirectObject> ToUnicodeAsync() => LowLevel.GetOrNullAsync(KnownNames.ToUnicode);
}