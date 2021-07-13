using Melville.Pdf.LowLevel.Visitors;

namespace Melville.Pdf.LowLevel.Model.Objects
{
    public class PdfIndirectReference : PdfObject
    {
        public PdfIndirectObject Target { get; }
        public PdfIndirectReference(PdfIndirectObject target)
        {
            Target = target;
        }

        public override PdfObject DirectValue() => Target.DirectValue();
        public override T Visit<T>(ILowLevelVisitor<T> visitor) => visitor.Visit(this);
    }
}