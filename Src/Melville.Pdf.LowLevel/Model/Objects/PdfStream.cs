using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Filters;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Visitors;

namespace Melville.Pdf.LowLevel.Model.Objects
{
    public interface IStreamDataSource
    {
        ValueTask<Stream> OpenRawStream(long streamLength);
        Stream WrapStreamWithDecryptor(Stream encryptedStream, PdfName cryptFilterName);
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

        public async ValueTask<Stream> GetEncodedStreamAsync() => 
            await source.OpenRawStream(await DeclaredLengthAsync());

        public async ValueTask<long> DeclaredLengthAsync() => 
            TryGetValue(KnownNames.Length, out var len) && await len is PdfNumber num ? num.IntValue : -1;

        public async ValueTask<Stream> GetDecodedStreamAsync(int desiredFormat = int.MaxValue) =>
            await Decoder.DecodeStream( await TryDecrypt(await GetEncodedStreamAsync()),
                await FilterList(), 
                await FilterParamList(),
                desiredFormat, source);

        private async ValueTask<IReadOnlyList<PdfObject>> FilterList() => 
            (await this.GetOrNullAsync(KnownNames.Filter)).AsList();
        private async ValueTask<IReadOnlyList<PdfObject>> FilterParamList() => 
            (await this.GetOrNullAsync(KnownNames.DecodeParms)).AsList();

        public async ValueTask<bool> HasFilterOfType(PdfName filterType)
        {
            foreach (var item in await FilterList())
            {
                if ((await item.DirectValue()) == filterType) return true;
            }
            return false;
        }

        private async ValueTask<Stream> TryDecrypt(Stream s)
        {
            return await this.GetOrNullAsync(KnownNames.Type) == KnownNames.XRef ||
                await HasFilterOfType(KnownNames.Crypt)? s : source.WrapStreamWithDecryptor(s, KnownNames.StmF);
        }

        public async ValueTask<IEnumerable<ObjectLocation>> GetInternalObjectNumbersAsync()
        {
            if (await this.GetOrNullAsync(KnownNames.Type) != KnownNames.ObjStm)
                return Array.Empty<ObjectLocation>();
            return await this.GetIncludedObjectNumbers();
        }
    }
}