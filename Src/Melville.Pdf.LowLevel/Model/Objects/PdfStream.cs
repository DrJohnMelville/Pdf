using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Filters.CryptFilters;
using Melville.Pdf.LowLevel.Filters.FilterProcessing;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.LowLevel.Visitors;

namespace Melville.Pdf.LowLevel.Model.Objects;

internal interface IStreamDataSource
{
    ValueTask<Stream> OpenRawStreamAsync(long streamLength);
    Stream WrapStreamWithDecryptor(Stream encryptedStream, PdfName cryptFilterName);
    Stream WrapStreamWithDecryptor(Stream encryptedStream);
    StreamFormat SourceFormat { get; }
}
    
/// <summary>
/// A PdfStream is a PDF object which contains a dictionary of attributes and
/// a binary stream of data.
/// </summary>
public sealed class PdfStream : PdfDictionary, IHasInternalIndirectObjects
{
    private readonly IStreamDataSource source;
        
    internal PdfStream(IStreamDataSource source, Memory<KeyValuePair<PdfName, PdfObject>> rawItems) :
        base(rawItems)
    {
        this.source = source;
    }

    internal override T Visit<T>(ILowLevelVisitor<T> visitor) => visitor.Visit(this);

    private async ValueTask<Stream> SourceStreamAsync() => 
        await source.OpenRawStreamAsync(await DeclaredLengthAsync().CA()).CA();

    /// <summary>
    /// The length of the stream as declared in the stream dictionary.
    /// </summary>
    /// <returns></returns>
    public ValueTask<long> DeclaredLengthAsync() => this.GetOrDefaultAsync(KnownNames.Length, -1);

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
        await DefaultEncryptionSelector.TryAddDefaultEncryptionAsync(this, source, innerEncryptor,
                new FilterProcessor(
                    await FilterListAsync().CA(),
                    await FilterParamListAsync().CA(),
                    CreateDecoder(innerEncryptor))).CA();

    private  IApplySingleFilter CreateDecoder(IObjectCryptContext innerEncryptor) =>
            new CryptSingleFilter(source, innerEncryptor,
                SinglePredictionFilter.Instance);

    private async ValueTask<IReadOnlyList<PdfObject>> FilterListAsync() => 
        (await this.GetOrNullAsync(KnownNames.Filter).CA()).ObjectAsUnresolvedList();
    private async ValueTask<IReadOnlyList<PdfObject>> FilterParamListAsync() => 
        (await this.GetOrNullAsync(KnownNames.DecodeParms).CA()).ObjectAsUnresolvedList();

    internal async ValueTask<bool> HasFilterOfTypeAsync(PdfName filterType)
    {
        foreach (var item in await FilterListAsync().CA())
        {
            if (await item.DirectValueAsync().CA() == filterType) return true;
        }
        return false;
    }

    async ValueTask<IEnumerable<ObjectLocation>> IHasInternalIndirectObjects.GetInternalObjectNumbersAsync()
    {
        if (await this.GetOrNullAsync(KnownNames.Type).CA() != KnownNames.ObjStm)
            return Array.Empty<ObjectLocation>();
        return await this.GetIncludedObjectNumbersAsync().CA();
    }
}