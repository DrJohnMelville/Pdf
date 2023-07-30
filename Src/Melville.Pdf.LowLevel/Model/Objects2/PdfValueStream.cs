using System;
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

public class PdfValueStream : PdfValueDictionary, IHasInternalIndirectObjects
{
    private readonly IStreamDataSource source;

    internal PdfValueStream(IStreamDataSource source, 
        Memory<KeyValuePair<PdfDirectValue, PdfIndirectValue>> values) : base(values)
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
        this.GetOrDefaultAsync(KnownNames.LengthTName, 0L);

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
            this, source, innerEncryptor,
            new FilterProcessor(
                await FilterListAsync().CA(),
                await FilterParamListAsync().CA(),
                CreateDecoder(innerEncryptor))).CA();

    private  IApplySingleFilter CreateDecoder(IObjectCryptContext innerEncryptor) =>
        new CryptSingleFilter(source, innerEncryptor,
            SinglePredictionFilter.Instance);

    private async ValueTask<IReadOnlyList<PdfIndirectValue>> FilterListAsync() => 
        (await this.GetOrNullAsync(KnownNames.FilterTName).CA()).ObjectAsUnresolvedList();
    private async ValueTask<IReadOnlyList<PdfIndirectValue>> FilterParamListAsync() => 
        (await this.GetOrNullAsync(KnownNames.DecodeParmsTName).CA()).ObjectAsUnresolvedList();




    protected virtual PdfDictionary TemporaryConvert(DictionaryBuilder builder) =>
        builder.AsStream(source);

    public async ValueTask<bool> HasFilterOfTypeAsync(PdfDirectValue filterType)
    {
        foreach (var item in await FilterListAsync().CA())
        {
            if ((await item.LoadValueAsync().CA()).Equals(filterType)) return true;
        }
        return false;
    }

    async ValueTask<IEnumerable<ObjectLocation>> IHasInternalIndirectObjects.GetInternalObjectNumbersAsync() =>
        (await this.GetOrNullAsync(KnownNames.TypeTName).CA()).Equals(KnownNames.ObjStmTName)
            ? await this.GetIncludedObjectNumbersAsync().CA()
            : Array.Empty<ObjectLocation>();
}