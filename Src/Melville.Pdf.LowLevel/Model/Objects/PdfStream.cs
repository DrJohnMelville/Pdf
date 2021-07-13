using System.Collections.Generic;
using Melville.Pdf.LowLevel.Visitors;

namespace Melville.Pdf.LowLevel.Model.Objects
{
    public class PdfStream : PdfDictionary
    {
        public long SourceFilePosition { get; }

        public PdfStream(IReadOnlyDictionary<PdfName, PdfObject> rawItems, long sourceFilePosition) : base(rawItems)
        {
            SourceFilePosition = sourceFilePosition;
        }
        
        public override T Visit<T>(ILowLevelVisitor<T> visitor) => visitor.Visit(this);
    }
}