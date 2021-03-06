using System.Threading.Tasks;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Parsing.ParserContext;

namespace Melville.Pdf.LowLevel.Parsing.ObjectParsers;

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
        SetFinalValue(await rentedReader.RootObjectParser.ParseAsync(rentedReader).CA());
    }
}