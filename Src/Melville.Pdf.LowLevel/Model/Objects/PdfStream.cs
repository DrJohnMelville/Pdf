using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Filters.CryptFilters;
using Melville.Pdf.LowLevel.Filters.FilterProcessing;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Visitors;

namespace Melville.Pdf.LowLevel.Model.Objects;

public interface IStreamDataSource
{
    ValueTask<Stream> OpenRawStream(long streamLength);
    Stream WrapStreamWithDecryptor(Stream encryptedStream, PdfName cryptFilterName);
    Stream WrapStreamWithDecryptor(Stream encryptedStream);
    StreamFormat SourceFormat { get; }
}
    
public sealed class PdfStream : PdfDictionary, IHasInternalIndirectObjects
{
    private IStreamDataSource source;
        
    public PdfStream(IStreamDataSource source, Memory<KeyValuePair<PdfName, PdfObject>> rawItems) :
        base(rawItems)
    {
        this.source = source;
    }

    internal override T Visit<T>(ILowLevelVisitor<T> visitor) => visitor.Visit(this);

    private async ValueTask<Stream> SourceStreamAsync() => 
        await source.OpenRawStream(await DeclaredLengthAsync().CA()).CA();

    public async ValueTask<long> DeclaredLengthAsync() => 
        TryGetValue(KnownNames.Length, out var len) && await len.CA() is PdfNumber num ? num.IntValue : -1;

    public ValueTask<Stream> StreamContentAsync(StreamFormat desiredFormat = StreamFormat.PlainText) =>
        StreamContentAsync(desiredFormat, null);
    internal async ValueTask<Stream> StreamContentAsync(StreamFormat desiredFormat, IObjectCryptContext? encryptor)
    {
        var processor =
            await CreateFilterProcessor(encryptor ?? ErrorObjectEncryptor.Instance).CA();
        
        return await processor.StreamInDesiredEncoding(await SourceStreamAsync().CA(),
            source.SourceFormat, desiredFormat).CA();
    }

    private async Task<FilterProcessorBase> CreateFilterProcessor(IObjectCryptContext innerEncryptor) =>
        await DefaultEncryptionSelector.TryAddDefaultEncryption(this, source, innerEncryptor,
                new FilterProcessor(
                    await FilterList().CA(),
                    await FilterParamList().CA(),
                    CreateDecoder(innerEncryptor))).CA();

    private  IApplySingleFilter CreateDecoder(IObjectCryptContext innerEncryptor) =>
            new CryptSingleFilter(source, innerEncryptor,
                SinglePredictionFilter.Instance);

    private async ValueTask<IReadOnlyList<PdfObject>> FilterList() => 
        (await this.GetOrNullAsync(KnownNames.Filter).CA()).ObjectAsUnresolvedList();
    private async ValueTask<IReadOnlyList<PdfObject>> FilterParamList() => 
        (await this.GetOrNullAsync(KnownNames.DecodeParms).CA()).ObjectAsUnresolvedList();

    public async ValueTask<bool> HasFilterOfType(PdfName filterType)
    {
        foreach (var item in await FilterList().CA())
        {
            if (await item.DirectValueAsync().CA() == filterType) return true;
        }
        return false;
    }

    public async ValueTask<IEnumerable<ObjectLocation>> GetInternalObjectNumbersAsync()
    {
        if (await this.GetOrNullAsync(KnownNames.Type).CA() != KnownNames.ObjStm)
            return Array.Empty<ObjectLocation>();
        return await this.GetIncludedObjectNumbersAsync().CA();
    }
}