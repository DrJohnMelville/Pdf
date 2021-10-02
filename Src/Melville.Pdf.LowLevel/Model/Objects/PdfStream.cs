using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Encryption.New;
using Melville.Pdf.LowLevel.Encryption.Readers;
using Melville.Pdf.LowLevel.Filters;
using Melville.Pdf.LowLevel.Filters.CryptFilters;
using Melville.Pdf.LowLevel.Filters.FilterProcessing;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Visitors;

namespace Melville.Pdf.LowLevel.Model.Objects
{
    public interface IStreamDataSource
    {
        ValueTask<Stream> OpenRawStream(long streamLength);
        Stream WrapStreamWithDecryptor(Stream encryptedStream, PdfName cryptFilterName);
        Stream WrapStreamWithDecryptor(Stream encryptedStream);
        StreamFormat SourceFormat { get; }
    }

    public class PdfStream : PdfDictionary, IHasInternalIndirectObjects
    {
        private IStreamDataSource source;
        
        public PdfStream(IStreamDataSource source, IReadOnlyDictionary<PdfName, PdfObject> rawItems) :
            base(rawItems)
        {
            this.source = source;
        }
        
        public PdfStream(IStreamDataSource source, params (PdfName, PdfObject)[] items) : 
            this(source, PairsToDictionary(items))
        {
        }

        public PdfStream(IStreamDataSource source, IEnumerable<(PdfName, PdfObject)> items): 
            this(source, PairsToDictionary(items))
        {
        }
        
        public override T Visit<T>(ILowLevelVisitor<T> visitor) => visitor.Visit(this);

        private async ValueTask<Stream> SourceStreamAsync() => 
            await source.OpenRawStream(await DeclaredLengthAsync());

        public async ValueTask<long> DeclaredLengthAsync() => 
            TryGetValue(KnownNames.Length, out var len) && await len is PdfNumber num ? num.IntValue : -1;

        public async ValueTask<Stream> StreamContentAsync(StreamFormat desiredFormat = StreamFormat.PlainText,
            IObjectCryptContext? encryptor = null)
        {
            var innerEncryptor = encryptor ?? ErrorObjectEncryptor.Instance;
            
            var decoder = new CryptSingleFilter(new SinglePredictionFilter(new StaticSingleFilter()),
                source, innerEncryptor);
            IFilterProcessor processor = 
                new FilterProcessor(await FilterList(), await FilterParamList(), decoder);
            if (await ShouldApplyDefaultEncryption())
            {
                processor = new DefaultEncryptionFilterProcessor(
                    processor, source, innerEncryptor);
            }
            return await processor.StreamInDesiredEncoding(await SourceStreamAsync(),
                source.SourceFormat, desiredFormat);
        }

        private async ValueTask<IReadOnlyList<PdfObject>> FilterList() => 
            (await this.GetOrNullAsync(KnownNames.Filter)).AsList();
        private async ValueTask<IReadOnlyList<PdfObject>> FilterParamList() => 
            (await this.GetOrNullAsync(KnownNames.DecodeParms)).AsList();

        public async ValueTask<bool> HasFilterOfType(PdfName filterType)
        {
            foreach (var item in await FilterList())
            {
                if (await item.DirectValueAsync() == filterType) return true;
            }
            return false;
        }

        private async Task<bool> ShouldApplyDefaultEncryption() =>
            !(await this.GetOrNullAsync(KnownNames.Type) == KnownNames.XRef ||
              await HasFilterOfType(KnownNames.Crypt));

        public async ValueTask<IEnumerable<ObjectLocation>> GetInternalObjectNumbersAsync()
        {
            if (await this.GetOrNullAsync(KnownNames.Type) != KnownNames.ObjStm)
                return Array.Empty<ObjectLocation>();
            return await this.GetIncludedObjectNumbersAsync();
        }
    }
}