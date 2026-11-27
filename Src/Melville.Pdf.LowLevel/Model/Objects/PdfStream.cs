using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

using Melville.Pdf.LowLevel.Filters.CryptFilters;
using Melville.Pdf.LowLevel.Filters.FilterProcessing;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects.StreamParts;
using Melville.StreamInterfaces;
using Melville.StreamInterfaces.Streams;

namespace Melville.Pdf.LowLevel.Model.Objects;

/// <summary>
/// This represents a PDF stream object.  It inherits from PdfDictionary because all
/// PDF streams are also dictionaries.  It has a few extra methods to retrieve the
/// associated data stream.
/// </summary>
public class PdfStream : PdfDictionary
{
    private readonly IStreamDataSource source;

    internal PdfStream(IStreamDataSource source, 
        Memory<KeyValuePair<PdfDirectObject, PdfIndirectObject>> values) : base(values)
    {
        this.source = source;
    }


    private async ValueTask<IPdfStreamReader> SourceStreamAsync()
    {
        var streamLength = await DeclaredLengthAsync().CA();
        return source.OpenRawReader(streamLength);
    }

    /// <summary>
    /// The on disk length of the stream as declared in the stream dictionary.
    /// </summary>
    public ValueTask<long> DeclaredLengthAsync() => 
        this.GetOrDefaultAsync(KnownNames.Length, 0L);

    /// <summary>
    /// Retrieve an IAsyncReader that reads the value od the sream decoded to the desired format.
    /// </summary>
    /// <param name="desiredFormat">The decoding level that is requested.</param>
    /// <param name="context">An optional context objext for the filters.</param>
    /// <returns></returns>
    public ValueTask<IPdfStreamReader> ReaderForContentAsync(
        StreamFormat desiredFormat = StreamFormat.PlainText, object? context = null) =>
           StreamEncryptedContentAsync(desiredFormat, null, context);

    internal async ValueTask<IPdfStreamReader> StreamEncryptedContentAsync(
        StreamFormat desiredFormat, IObjectCryptContext? encryptor, object? context)
    {
        Debug.Assert(context is not IObjectCryptContext);
        var processor =
            await CreateFilterProcessorAsync(encryptor ?? ErrorObjectEncryptor.Instance, context).CA();
        
        return (await processor.StreamInDesiredEncodingAsync((await SourceStreamAsync().CA()),
            source.SourceFormat, desiredFormat).CA());
    }

    private async Task<FilterProcessorBase> CreateFilterProcessorAsync(
        IObjectCryptContext innerEncryptor, object? context) =>
        await DefaultEncryptionSelector.TryAddDefaultEncryptionAsync(
            this, source, innerEncryptor,
            new FilterProcessor(
                await FilterListAsync().CA(),
                await FilterParamListAsync().CA(),
                CreateDecoder(innerEncryptor), context)).CA();

    private  IApplySingleFilter CreateDecoder(IObjectCryptContext innerEncryptor) =>
        new CryptSingleFilter(source, innerEncryptor,
            SinglePredictionFilter.Instance);

    private async ValueTask<IReadOnlyList<PdfIndirectObject>> FilterListAsync() => 
        (await this.GetOrNullAsync(KnownNames.Filter).CA()).ObjectAsUnresolvedList();
    private async ValueTask<IReadOnlyList<PdfIndirectObject>> FilterParamListAsync() => 
        (await this.GetOrNullAsync(KnownNames.DecodeParms).CA()).ObjectAsUnresolvedList();


    internal async ValueTask<bool> HasFilterOfTypeAsync(PdfDirectObject filterType)
    {
        foreach (var item in await FilterListAsync().CA())
        {
            if ((await item.LoadValueAsync().CA()).Equals(filterType)) return true;
        }
        return false;
    }
}