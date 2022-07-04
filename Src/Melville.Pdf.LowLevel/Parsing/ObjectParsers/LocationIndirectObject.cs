using System.Threading.Tasks;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Parsing.ParserContext;

namespace Melville.Pdf.LowLevel.Parsing.ObjectParsers;

public class LocationIndirectObject : OwnerContainedIndirectObject
{
    private readonly long offset;
 
    public LocationIndirectObject(int objectNumber, int generationNumber, long offset, ParsingFileOwner owner) : 
        base(objectNumber, generationNumber, owner)
    {
        this.offset = offset;
    }


    protected override async ValueTask ReadFinalValue(ParsingFileOwner localOwner)
    {
        var rentedReader = await localOwner.RentReader(offset, ObjectNumber, (GenerationNumber)).CA();
        value = await rentedReader.RootObjectParser.ParseAsync(rentedReader).CA();
    }
}