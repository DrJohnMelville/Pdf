using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Visitors;

namespace Melville.Pdf.LowLevel.Model.Objects
{
    public interface IStreamDataSource
    {
        ValueTask<Stream> OpenRawStream(long streamLength);
    }
    
    public class PdfStream : PdfDictionary
    {
        private IStreamDataSource source;
        
        public PdfStream(IReadOnlyDictionary<PdfName, PdfObject> rawItems, IStreamDataSource source) :
            base(rawItems)
        {
            this.source = source;
        }
        
        public override T Visit<T>(ILowLevelVisitor<T> visitor) => visitor.Visit(this);

        public  ValueTask<Stream> GetRawStream()
        {
            return source.OpenRawStream(DeclaredLength);
        }

        public long DeclaredLength => 
            TryGetValue(KnownNames.Length, out var len) && len is PdfNumber num ? num.IntValue : -1;
    }
}