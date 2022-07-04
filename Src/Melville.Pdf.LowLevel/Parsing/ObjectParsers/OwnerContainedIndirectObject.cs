using System.Threading.Tasks;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Parsing.ParserContext;

namespace Melville.Pdf.LowLevel.Parsing.ObjectParsers;

public abstract class OwnerContainedIndirectObject : PdfIndirectObject
{
    protected volatile ParsingFileOwner? owner;

    protected OwnerContainedIndirectObject(int objectNumber, int generationNumber,ParsingFileOwner? owner) : 
        base(objectNumber, generationNumber, PdfTokenValues.ArrayTerminator)
    {
        this.owner = owner;
    }

    public override async ValueTask<PdfObject> DirectValueAsync()
    {
        if (owner is { } localOwner)
        {
            await ReadFinalValue(localOwner);
            owner = null;
        }
        return await base.DirectValueAsync().CA();
    }
    public override bool TryGetDirectValue(out PdfObject result)
    {
        result = value;
        return owner != null;
    }

    protected abstract ValueTask ReadFinalValue(ParsingFileOwner localOwner);
}