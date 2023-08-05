using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Filters.CryptFilters;
using Melville.Pdf.LowLevel.Filters.FilterProcessing;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects.StreamParts;

namespace Melville.Pdf.LowLevel.Model.Objects;

/// <summary>
/// This represents a PDF stream object.  It inherits from PdfDictionary because all
/// PDF streams are also dictionaries.  It has a few extra methods to retrieve the
/// associated data strea,.
/// </summary>
public class PdfStream : PdfDictionary, IHasInternalIndirectObjects
{
    private readonly IStreamDataSource source;

    internal PdfStream(IStreamDataSource source, 
        Memory<KeyValuePair<PdfDirectObject, PdfIndirectObject>> values) : base(values)
    {
        this.source = source;
    }

    private async ValueTask<Stream> SourceStreamAsync() => 
        await source.OpenRawStreamAsync(await DeclaredLengthAsync().CA()).CA();

    /// <summary>
    /// The on disk length of the stream as declared in the stream dictionary.
    /// </summary>
    public ValueTask<long> DeclaredLengthAsync() => 
        this.GetOrDefaultAsync(KnownNames.Length, 0L);

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

    private async ValueTask<IReadOnlyList<PdfIndirectObject>> FilterListAsync() => 
        (await this.GetOrNullAsync(KnownNames.Filter).CA()).ObjectAsUnresolvedList();
    private async ValueTask<IReadOnlyList<PdfIndirectObject>> FilterParamListAsync() => 
        (await this.GetOrNullAsync(KnownNames.DecodeParms).CA()).ObjectAsUnresolvedList();


    public async ValueTask<bool> HasFilterOfTypeAsync(PdfDirectObject filterType)
    {
        foreach (var item in await FilterListAsync().CA())
        {
            if ((await item.LoadValueAsync().CA()).Equals(filterType)) return true;
        }
        return false;
    }

    async ValueTask<IEnumerable<ObjectLocation>> IHasInternalIndirectObjects.GetInternalObjectNumbersAsync() =>
        (await this.GetOrNullAsync(KnownNames.Type).CA()).Equals(KnownNames.ObjStm)
            ? await this.GetIncludedObjectNumbersAsync().CA()
            : Array.Empty<ObjectLocation>();
}