using Melville.Pdf.LowLevel.Visitors;

namespace Melville.Pdf.LowLevel.Model.Objects
{
    public abstract class PdfObject
    {
        public virtual PdfObject DirectValue() => this;
        public abstract T Visit<T>(ILowLevelVisitor<T> visitor);
    }
}