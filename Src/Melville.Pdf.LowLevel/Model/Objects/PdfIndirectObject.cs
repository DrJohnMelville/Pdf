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
}
public class PdfIndirectObject: PdfObject, IMultableIndirectObject
{
    public int ObjectNumber { get; }
    public int GenerationNumber { get; }
    protected PdfObject value = PdfTokenValues.Null;

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

    void IMultableIndirectObject.SetValue(PdfObject value)
    {
        this.value = value;
    }

    bool IMultableIndirectObject.HasRegisteredAccessor() => value != PdfTokenValues.ArrayTerminator;
}

public class IndirectObjectWithAccessor : PdfIndirectObject
{
    private Func<ValueTask<PdfObject>>? accessor;

    public IndirectObjectWithAccessor(int objectNumber, int generationNumber, Func<ValueTask<PdfObject>>? accessor) : 
        base(objectNumber, generationNumber, PdfTokenValues.Null)
    {
        this.accessor = accessor;
    }
    public override async ValueTask<PdfObject> DirectValueAsync()
    {
        if (accessor != null)
        {
            value = await accessor().CA();
            accessor = null;
        } 
        return value = await value.DirectValueAsync().CA();
    }

    public override bool TryGetDirectValue(out PdfObject result)
    {
        result = value;
        return accessor == null;
    }
}

public class UnknownIndirectObject : PdfIndirectObject
{
    public UnknownIndirectObject(int objectNumber, int generationNumber) : 
        base(objectNumber, generationNumber, PdfTokenValues.ArrayTerminator)
    {
    }
}