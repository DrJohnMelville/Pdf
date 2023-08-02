using System;
using System.Threading.Tasks;
using Melville.INPC;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Postscript.Interpreter.Values;

namespace Melville.Pdf.Model.Documents;

internal readonly partial struct PdfFont
{
    [FromConstructor]public readonly PdfDictionary LowLevel { get; }

    public PdfDirectObject SubType() => LowLevel.SubTypeOrNull(KnownNames.Type1TName);
    
    public async ValueTask<PdfEncoding> EncodingAsync() => 
        new(await LowLevel.GetOrNullAsync(KnownNames.EncodingTName).CA());

    private ValueTask<PdfDictionary?> DescriptorAsync() =>
        LowLevel.GetOrNullAsync<PdfDictionary>(KnownNames.FontDescriptorTName);

    public async ValueTask<FontFlags> FontFlagsAsync() =>
        await DescriptorAsync().CA() is { } descriptor
            ? (FontFlags)(await descriptor.GetOrDefaultAsync(KnownNames.FlagsTName, 0).CA())
            : FontFlags.None;

    public async ValueTask<PdfStream?> EmbeddedStreamAsync() =>
        await DescriptorAsync().CA() is { } descriptor
            && (
            descriptor.TryGetValue(KnownNames.FontFileTName, out var retTask) ||
                descriptor.TryGetValue(KnownNames.FontFile2TName, out retTask) ||
              descriptor.TryGetValue(KnownNames.FontFile3TName, out retTask) )? 
                (await retTask.CA()).Get<PdfStream>(): null;

    public ValueTask<PdfDirectObject> BaseFontNameAsync() =>
        LowLevel.GetOrDefaultAsync(KnownNames.BaseFontTName, KnownNames.HelveticaTName);

    public async ValueTask<byte> FirstCharAsync() =>
        (byte)await LowLevel.GetOrDefaultAsync(KnownNames.FirstCharTName, 0).CA();

    public async ValueTask<double[]?> WidthsArrayAsync() =>
        (await LowLevel.GetOrNullAsync<PdfArray>(KnownNames.WidthsTName).CA()) is { } widthArray
            ? await widthArray.CastAsync<double>().CA()
            : null;
    

    public async ValueTask<PdfDirectObject> OsFontNameAsync() =>
        ComputeOsFontName(SubType(), await BaseFontNameAsync().CA());
    
    private PdfDirectObject ComputeOsFontName(PdfDirectObject fontType, PdfDirectObject baseFontName) =>
        fontType.Equals(KnownNames.MMType1TName)?
            RemoveMultMasterSuffix(baseFontName):baseFontName;
    
    private PdfDirectObject RemoveMultMasterSuffix(PdfDirectObject baseFontName)
    {
        Span<byte> source = baseFontName.Get<StringSpanSource>().GetSpan();
        var firstUnderscore = source.IndexOf((byte)'_');
        return firstUnderscore < 0 ? baseFontName : PdfDirectObject.CreateName(source[..firstUnderscore]);
    }

    public async ValueTask<PdfFont> Type0SubFontAsync() =>
        new PdfFont(
            await (await LowLevel.GetAsync<PdfArray>(KnownNames.DescendantFontsTName).CA())
                .GetAsync<PdfDictionary>(0).CA()
        );

    public ValueTask<PdfDictionary?> CidSystemInfoAsync() =>
        LowLevel.GetOrDefaultAsync(KnownNames.CIDSystemInfoTName, (PdfDictionary?)null);

    public ValueTask<PdfStream?> CidToGidMapStreamAsync() => 
        LowLevel.GetOrDefaultAsync(KnownNames.CIDToGIDMapTName, (PdfStream?)null);

    public ValueTask<double> DefaultWidthAsync() =>
        LowLevel.GetOrDefaultAsync(KnownNames.DWTName, 1000.0);

    public ValueTask<PdfArray> WArrayAsync() =>
        LowLevel.GetOrDefaultAsync(KnownNames.WTName, PdfArray.Empty);
}

