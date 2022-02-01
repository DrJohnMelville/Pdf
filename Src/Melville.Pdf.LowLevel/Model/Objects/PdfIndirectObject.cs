using System;
using System.Threading.Tasks;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.LowLevel.Visitors;

namespace Melville.Pdf.LowLevel.Model.Objects;

internal interface IMultableIndirectObject
{
    bool HasRegisteredAccessor();
    void SetValue(PdfObject value);
    void SetValue(Func<ValueTask<PdfObject>> value);

}
public class PdfIndirectObject: PdfObject, IMultableIndirectObject
{
    public int ObjectNumber { get; }
    public int GenerationNumber { get; }
    private Func<ValueTask<PdfObject>>? accessor = null;
    private PdfObject value = PdfTokenValues.Null;


    public PdfIndirectObject(
        int objectNumber, int generationNumber, Func<ValueTask<PdfObject>>? accessor)
    {
        this.accessor = accessor;
        ObjectNumber = objectNumber;
        GenerationNumber = generationNumber;
    }

    public PdfIndirectObject(int objectNumber, int generationNumber, PdfObject value)
    {
        ObjectNumber = objectNumber;
        GenerationNumber = generationNumber;
        this.value = value;
    }
        
    public override async ValueTask<PdfObject> DirectValueAsync()
    {
        if (accessor != null)
        {
            value = await accessor().CA();
            accessor = null;
        } 
        return value;
    }

    public bool TryGetDirectValue(out PdfObject result)
    {
        result = value;
        return accessor == null;
    }
    
    public override T Visit<T>(ILowLevelVisitor<T> visitor) => visitor.Visit(this);

    void IMultableIndirectObject.SetValue(Func<ValueTask<PdfObject>> value) =>
        accessor = value;
    void IMultableIndirectObject.SetValue(PdfObject value)
    {
        if (value is PdfIndirectObject)
            throw new PdfParseException("Double indirect references are not allowed.");
        this.value = value;
        accessor = null;
    }

    bool IMultableIndirectObject.HasRegisteredAccessor() => 
        accessor != null && value == PdfTokenValues.Null;
}