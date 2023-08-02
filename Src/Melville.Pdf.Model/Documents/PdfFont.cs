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
    [FromConstructor]public readonly PdfValueDictionary LowLevel { get; }

    public PdfDirectValue SubType() => LowLevel.SubTypeOrNull(KnownNames.Type1TName);
    
    public async ValueTask<PdfEncoding> EncodingAsync() => 
        new(await LowLevel.GetOrNullAsync(KnownNames.EncodingTName).CA());

    private ValueTask<PdfValueDictionary?> DescriptorAsync() =>
        LowLevel.GetOrNullAsync<PdfValueDictionary>(KnownNames.FontDescriptorTName);

    public async ValueTask<FontFlags> FontFlagsAsync() =>
        await DescriptorAsync().CA() is { } descriptor
            ? (FontFlags)(await descriptor.GetOrDefaultAsync(KnownNames.FlagsTName, 0).CA())
            : FontFlags.None;

    public async ValueTask<PdfValueStream?> EmbeddedStreamAsync() =>
        await DescriptorAsync().CA() is { } descriptor
            && (
            descriptor.TryGetValue(KnownNames.FontFileTName, out var retTask) ||
                descriptor.TryGetValue(KnownNames.FontFile2TName, out retTask) ||
              descriptor.TryGetValue(KnownNames.FontFile3TName, out retTask) )? 
                (await retTask.CA()).Get<PdfValueStream>(): null;

    public ValueTask<PdfDirectValue> BaseFontNameAsync() =>
        LowLevel.GetOrDefaultAsync(KnownNames.BaseFontTName, KnownNames.HelveticaTName);

    public async ValueTask<byte> FirstCharAsync() =>
        (byte)await LowLevel.GetOrDefaultAsync(KnownNames.FirstCharTName, 0).CA();

    public async ValueTask<double[]?> WidthsArrayAsync() =>
        (await LowLevel.GetOrNullAsync<PdfValueArray>(KnownNames.WidthsTName).CA()) is { } widthArray
            ? await widthArray.CastAsync<double>().CA()
            : null;
    

    public async ValueTask<PdfDirectValue> OsFontNameAsync() =>
        ComputeOsFontName(SubType(), await BaseFontNameAsync().CA());
    
    private PdfDirectValue ComputeOsFontName(PdfDirectValue fontType, PdfDirectValue baseFontName) =>
        fontType.Equals(KnownNames.MMType1TName)?
            RemoveMultMasterSuffix(baseFontName):baseFontName;
    
    private PdfDirectValue RemoveMultMasterSuffix(PdfDirectValue baseFontName)
    {
        Span<byte> source = baseFontName.Get<StringSpanSource>().GetSpan();
        var firstUnderscore = source.IndexOf((byte)'_');
        return firstUnderscore < 0 ? baseFontName : PdfDirectValue.CreateName(source[..firstUnderscore]);
    }

    public async ValueTask<PdfFont> Type0SubFontAsync() =>
        new PdfFont(
            await (await LowLevel.GetAsync<PdfValueArray>(KnownNames.DescendantFontsTName).CA())
                .GetAsync<PdfValueDictionary>(0).CA()
        );

    public ValueTask<PdfValueDictionary?> CidSystemInfoAsync() =>
        LowLevel.GetOrDefaultAsync(KnownNames.CIDSystemInfoTName, (PdfValueDictionary?)null);

    public ValueTask<PdfValueStream?> CidToGidMapStreamAsync() => 
        LowLevel.GetOrDefaultAsync(KnownNames.CIDToGIDMapTName, (PdfValueStream?)null);

    public ValueTask<double> DefaultWidthAsync() =>
        LowLevel.GetOrDefaultAsync(KnownNames.DWTName, 1000.0);

    public ValueTask<PdfValueArray> WArrayAsync() =>
        LowLevel.GetOrDefaultAsync(KnownNames.WTName, PdfValueArray.Empty);
}

