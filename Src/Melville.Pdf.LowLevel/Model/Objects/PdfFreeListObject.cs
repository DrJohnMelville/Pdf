using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Visitors;

namespace Melville.Pdf.LowLevel.Model.Objects;

public class PdfFreeListObject : PdfIndirectObject
{
    public PdfFreeListObject(int num, int generation, long nextItem): 
        base(num, generation, PdfTokenValues.DictionaryTerminator)
    {
        NextItem = nextItem;
    }

    public long NextItem { get; }
    public override T Visit<T>(ILowLevelVisitor<T> visitor) => visitor.Visit(this);
    public override bool ShouldWriteToFile() => false;
    public override string ToString() => $"Free Item. Next = " + NextItem;
    
          
    public override ValueTask<PdfObject> DirectValueAsync() => new(this);

    public override bool TryGetDirectValue(out PdfObject result)
    {
        result = this;
        return true;
    }

}