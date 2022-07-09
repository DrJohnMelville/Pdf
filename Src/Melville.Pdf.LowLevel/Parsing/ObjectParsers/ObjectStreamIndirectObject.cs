using System.Threading.Tasks;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Parsing.ParserContext;

namespace Melville.Pdf.LowLevel.Parsing.ObjectParsers;

public class ObjectStreamIndirectObject : OwnedLocationIndirectObject
{
    private readonly long referredOrdinal;

    public ObjectStreamIndirectObject(
        int objectNumber, int generationNumber, ParsingFileOwner owner, 
        long referredOrdinal) : 
        base(objectNumber, generationNumber, owner)
    {
        this.referredOrdinal = referredOrdinal;
    }

    protected override async ValueTask ComputeValue(ParsingFileOwner owner)
    {
        var referredObject = await owner.IndirectResolver
            .FindIndirect((int)referredOrdinal, 0).DirectValueAsync().CA();
        if (referredObject is PdfStream stream)
            await LoadObjectStream(owner, stream).CA();
    }
    
    public static async ValueTask LoadObjectStream(ParsingFileOwner owner, PdfStream source)
    {
        await using var data = await source.StreamContentAsync().CA();
        var reader = owner.ParsingReaderForStream(data, 0);
        var objectLocations = await ObjectStreamOperations.GetIncludedObjectNumbers(
            source, reader.Reader).CA();
        var first = (await source.GetAsync<PdfNumber>(KnownNames.First).CA()).IntValue;
        foreach (var location in objectLocations)
        {
            await reader.Reader.Source.AdvanceToLocalPositionAsync(first + location.Offset).CA();
            var obj = await PdfParserParts.Composite.ParseAsync(reader).CA();
            AcceptObject(owner.IndirectResolver,location.ObjectNumber,obj);
        }
    }

    private static void AcceptObject(IIndirectObjectResolver resolver,
        int objectNumber, PdfObject pdfObject)
    {
        var obj = resolver.FindIndirect(objectNumber, 0)as ObjectStreamIndirectObject;
        obj?.SetFinalValue(pdfObject);
    }
}