using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Filters.CryptFilters;
using Melville.Pdf.LowLevel.Filters.FilterProcessing;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;

namespace Melville.Pdf.LowLevel.Model.Objects2;

public class PdfValueStream : PdfValueDictionary
{
    private readonly IStreamDataSource source;

    internal PdfValueStream(IStreamDataSource source, 
        KeyValuePair<PdfDirectValue, PdfIndirectValue>[] values) : base(values)
    {
        this.source = source;
    }

    private async ValueTask<Stream> SourceStreamAsync() => 
        await source.OpenRawStreamAsync(await DeclaredLengthAsync().CA()).CA();

    /// <summary>
    /// The length of the stream as declared in the stream dictionary.
    /// </summary>
    /// <returns></returns>
    public ValueTask<long> DeclaredLengthAsync() => 
        this.GetOrDefaultAsync(KnownNames.LengthTName, -1L);

    /// <summary>
    /// Retrieves a C# stream that will read the stream contents in the desired format.
    /// </summary>
    /// <param name="desiredFormat">The filters that should be applied to the read stream.</param>
    /// <returns></returns>
    public ValueTask<Stream> StreamContentAsync(StreamFormat desiredFormat = StreamFormat.PlainText) =>
        StreamContentAsync(desiredFormat, null);

    internal async ValueTask<Stream> StreamContentAsync(StreamFormat desiredFormat, IObjectCryptContext? encryptor)
    {
        var processor =
            await CreateFilterProcessorAsync(encryptor ?? ErrorObjectEncryptor.Instance).CA();
        
        return await processor.StreamInDesiredEncodingAsync(await SourceStreamAsync().CA(),
            source.SourceFormat, desiredFormat).CA();
    }

    private async Task<FilterProcessorBase> CreateFilterProcessorAsync(IObjectCryptContext innerEncryptor) =>
        await DefaultEncryptionSelector.TryAddDefaultEncryptionAsync(
            (PdfStream)TemporaryConvert(), source, innerEncryptor,
            new FilterProcessor(
                await FilterListAsync().CA(),
                await FilterParamListAsync().CA(),
                CreateDecoder(innerEncryptor))).CA();

    private  IApplySingleFilter CreateDecoder(IObjectCryptContext innerEncryptor) =>
        new CryptSingleFilter(source, innerEncryptor,
            SinglePredictionFilter.Instance);

    private async ValueTask<IReadOnlyList<PdfObject>> FilterListAsync() => 
        (await this.GetOrNullAsync(KnownNames.FilterU8).CA()).AsOldObject().ObjectAsUnresolvedList();
    private async ValueTask<IReadOnlyList<PdfObject>> FilterParamListAsync() => 
        (await this.GetOrNullAsync(KnownNames.DecodeParmsU8).CA()).AsOldObject().ObjectAsUnresolvedList();




    protected override PdfDictionary TemporaryConvert(DictionaryBuilder builder) =>
        builder.AsStream(source);
}