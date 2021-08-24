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
    }
    
    public class PdfStream : PdfDictionary, IHasInternalIndirectObjects
    {
        private IStreamDataSource source;
        
        public PdfStream(IReadOnlyDictionary<PdfName, PdfObject> rawItems, IStreamDataSource source) :
            base(rawItems)
        {
            this.source = source;
        }
        
        public override T Visit<T>(ILowLevelVisitor<T> visitor) => visitor.Visit(this);

        public async ValueTask<Stream> GetEncodedStreamAsync()
        {
            return await source.OpenRawStream(await DeclaredLengthAsync());
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