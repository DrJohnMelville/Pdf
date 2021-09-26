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
        ValueTask<Stream> OpenRawStream(long streamLength, PdfStream stream);
        Stream WrapStreamWithDecryptor(Stream encryptedStream, PdfName? cryptFilterName);
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

        public async ValueTask<Stream> GetEncodedStreamAsync()
        {
            return await source.OpenRawStream(await DeclaredLengthAsync(), this);
        }

        public async ValueTask<long> DeclaredLengthAsync() => 
            TryGetValue(KnownNames.Length, out var len) && await len is PdfNumber num ? num.IntValue : -1;

        public async ValueTask<Stream> GetDecodedStreamAsync(int desiredFormat = int.MaxValue) =>
            await Decoder.DecodeStream(await GetEncodedStreamAsync(),
                (await this.GetOrNullAsync(KnownNames.Filter)).AsList(), 
                (await this.GetOrNullAsync(KnownNames.DecodeParms)).AsList(),
                desiredFormat);

        public async ValueTask<IEnumerable<ObjectLocation>> GetInternalObjectNumbersAsync()
        {
            if (await this.GetOrNullAsync(KnownNames.Type) != KnownNames.ObjStm)
                return Array.Empty<ObjectLocation>();
            return await this.GetIncludedObjectNumbers();
        }
    }
}