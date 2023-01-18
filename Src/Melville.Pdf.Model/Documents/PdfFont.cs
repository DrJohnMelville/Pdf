using System;
using System.Threading.Tasks;
using Melville.INPC;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.Model.Documents;

internal readonly partial struct PdfFont
{
    [FromConstructor]public readonly PdfDictionary LowLevel { get; }

    public PdfName SubType() => LowLevel.SubTypeOrNull() ?? KnownNames.Type1;
    
    public async ValueTask<PdfEncoding> EncodingAsync() => 
        new(await LowLevel.GetOrNullAsync(KnownNames.Encoding).CA());

    private ValueTask<PdfDictionary?> DescriptorAsync() =>
        LowLevel.GetOrNullAsync<PdfDictionary>(KnownNames.FontDescriptor);

    public async ValueTask<FontFlags> FontFlagsAsync() =>
        await DescriptorAsync().CA() is { } descriptor
            ? (FontFlags)(await descriptor.GetOrDefaultAsync(KnownNames.Flags, 0).CA())
            : FontFlags.None;

    public async ValueTask<PdfStream?> EmbeddedStreamAsync() =>
        await DescriptorAsync().CA() is { } descriptor
            && (
            descriptor.TryGetValue(KnownNames.FontFile, out var retTask) ||
                descriptor.TryGetValue(KnownNames.FontFile2, out retTask) ||
              descriptor.TryGetValue(KnownNames.FontFile3, out retTask) )? 
                await retTask.CA() as PdfStream : null;

    public ValueTask<PdfName> BaseFontNameAsync() =>
        LowLevel.GetOrDefaultAsync(KnownNames.BaseFont, KnownNames.Helvetica);

    public async ValueTask<byte> FirstCharAsync() =>
        (byte)await LowLevel.GetOrDefaultAsync(KnownNames.FirstChar, 0).CA();

    public async ValueTask<double[]?> WidthsArrayAsync() =>
        (await LowLevel.GetOrNullAsync<PdfArray>(KnownNames.Widths).CA()) is { } widthArray
            ? await widthArray.AsDoublesAsync().CA()
            : null;
    

    public async ValueTask<PdfName> OsFontNameAsync() =>
        ComputeOsFontName(SubType().GetHashCode(), await BaseFontNameAsync().CA());
    
    private PdfName ComputeOsFontName(int fontType, PdfName baseFontName) =>
        fontType == KnownNameKeys.MMType1?
            RemoveMultMasterSuffix(baseFontName):baseFontName;
    
    private PdfName RemoveMultMasterSuffix(PdfName baseFontName)
    {
        var source = baseFontName.Bytes.AsSpan();
        var firstUnderscore = source.IndexOf((byte)'_');
        return firstUnderscore < 0 ? baseFontName : NameDirectory.Get(source[..firstUnderscore]);
    }

    public async ValueTask<PdfFont> Type0SubFont() =>
        new PdfFont(
            await (await LowLevel.GetAsync<PdfArray>(KnownNames.DescendantFonts).CA())
                .GetAsync<PdfDictionary>(0).CA()
        );

    public ValueTask<PdfDictionary?> CidSystemInfo() =>
        LowLevel.GetOrDefaultAsync(KnownNames.CIDSystemInfo, (PdfDictionary?)null);

    public ValueTask<PdfStream?> CidToGidMapStream() => 
        LowLevel.GetOrDefaultAsync(KnownNames.CIDToGIDMap, (PdfStream?)null);

    public ValueTask<double> DefaultWidthAsync() =>
        LowLevel.GetOrDefaultAsync(KnownNames.DW, 1000.0);

    public ValueTask<PdfArray> WArrayAsync() =>
        LowLevel.GetOrDefaultAsync(KnownNames.W, PdfArray.Empty);
}

