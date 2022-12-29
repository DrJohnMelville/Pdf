using Melville.Pdf.LowLevel.Visitors;

namespace Melville.Pdf.LowLevel.Model.Objects;

public class PdfFreeListObject : PdfObject
{
    public PdfFreeListObject(long nextItem)
    {
        NextItem = nextItem;
    }

    public long NextItem { get; }
    internal override T Visit<T>(ILowLevelVisitor<T> visitor) => visitor.Visit(this);
    public override bool ShouldWriteToFile() => false;
    public override string ToString() => $"Free Item. Next = " + NextItem;
}