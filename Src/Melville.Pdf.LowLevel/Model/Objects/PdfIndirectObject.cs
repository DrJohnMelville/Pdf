using System.Threading.Tasks;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.LowLevel.Visitors;

namespace Melville.Pdf.LowLevel.Model.Objects;

public class PdfIndirectObject: PdfObject
{
    public int ObjectNumber { get; }
    public int GenerationNumber { get; }
    protected PdfObject value;

    public PdfIndirectObject(int objectNumber, int generationNumber, PdfObject value)
    {
        ObjectNumber = objectNumber;
        GenerationNumber = generationNumber;
        this.value = value;
    }
        
    public override async ValueTask<PdfObject> DirectValueAsync()
    {
        return value = await value.DirectValueAsync().CA();
    }

    public virtual bool TryGetDirectValue(out PdfObject result)
    {
        result = value;
        return true;
    }
    
    public override T Visit<T>(ILowLevelVisitor<T> visitor) => visitor.Visit(this);

}

public class UnknownIndirectObject : PdfIndirectObject
{
    public UnknownIndirectObject(int objectNumber, int generationNumber) : 
        base(objectNumber, generationNumber, PdfTokenValues.ArrayTerminator)
    {
    }
    public void SetValue(PdfObject value)
    {
        this.value = value;
    }

    public bool HasRegisteredAccessor() => value != PdfTokenValues.ArrayTerminator;
}