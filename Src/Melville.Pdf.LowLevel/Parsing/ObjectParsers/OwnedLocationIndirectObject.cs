using System.Threading.Tasks;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Parsing.ParserContext;

namespace Melville.Pdf.LowLevel.Parsing.ObjectParsers;

public abstract class OwnedLocationIndirectObject: PdfIndirectObject
{
    private ParsingFileOwner? owner;

    protected OwnedLocationIndirectObject(int objectNumber, int generationNumber, ParsingFileOwner owner) : 
        base(objectNumber, generationNumber, PdfTokenValues.Null)
    {
        this.owner = owner;
    }

    public override async ValueTask<PdfObject> DirectValueAsync()
    {
        if (owner is { } localOwner)
        {
            await ComputeValue(localOwner).CA();
            owner = null;
        }
        return await base.DirectValueAsync().CA();
    }

    public override bool TryGetDirectValue(out PdfObject result)
    {
        result = value;
        return owner == null;
    }

    protected abstract ValueTask ComputeValue(ParsingFileOwner owner);
}

public class RawLocationIndirectObject : OwnedLocationIndirectObject
{
    private readonly long location;

    public RawLocationIndirectObject(int objectNumber, int generationNumber, ParsingFileOwner owner, long location) : base(objectNumber, generationNumber, owner)
    {
        this.location = location;
    }

    protected override async ValueTask ComputeValue(ParsingFileOwner owner)
    {
        var rentedReader = await owner.RentReader(location, ObjectNumber, GenerationNumber).CA();
        value = await rentedReader.RootObjectParser.ParseAsync(rentedReader).CA();
    }
}