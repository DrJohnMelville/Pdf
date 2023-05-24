using System.Diagnostics;
using System.Threading.Tasks;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Parsing.ParserContext;

namespace Melville.Pdf.LowLevel.Parsing.ObjectParsers;

internal abstract class OwnedLocationIndirectObject: PdfIndirectObject
{
    private ParsingFileOwner? owner;

    protected OwnedLocationIndirectObject(int objectNumber, int generationNumber, ParsingFileOwner owner) : 
        base(objectNumber, generationNumber, PdfTokenValues.Null)
    {
        Debug.Assert(owner != null);
        this.owner = owner;
    }

    public override async ValueTask<PdfObject> DirectValueAsync()
    {
        if (owner is { } localOwner)
        {
            await ComputeValueAsync(localOwner).CA();
        }
        return await base.DirectValueAsync().CA();
    }

    internal override bool TryGetDirectValue(out PdfObject result)
    {
        result = value;
        return HasFinalValue();
    }

    private bool HasFinalValue() => owner == null;

    protected void SetFinalValue(PdfObject value)
    {
        this.value = value;
        owner = null;
    }

    protected abstract ValueTask ComputeValueAsync(ParsingFileOwner owner);
}