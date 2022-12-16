using System;
using System.Threading.Tasks;
using Melville.Parsing.AwaitConfiguration;

namespace Melville.Pdf.LowLevel.Model.Objects;

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