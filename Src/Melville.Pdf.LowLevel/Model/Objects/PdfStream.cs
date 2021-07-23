﻿using System.Collections.Generic;
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

        public async ValueTask<Stream> GetRawStream()
        {
            return await source.OpenRawStream(await DeclaredLength());
        }

        public async ValueTask<long> DeclaredLength() => 
            TryGetValue(KnownNames.Length, out var len) && await len is PdfNumber num ? num.IntValue : -1;
    }
}