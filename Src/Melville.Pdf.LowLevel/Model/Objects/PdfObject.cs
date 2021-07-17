using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Visitors;

namespace Melville.Pdf.LowLevel.Model.Objects
{
    public abstract class PdfObject
    {
        public virtual ValueTask<PdfObject> DirectValue() => new ValueTask<PdfObject>(this);
        public abstract T Visit<T>(ILowLevelVisitor<T> visitor);
    }
}