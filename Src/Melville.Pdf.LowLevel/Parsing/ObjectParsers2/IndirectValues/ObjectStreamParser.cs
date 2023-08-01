using System.Collections.Generic;
using System.IO.Pipelines;
using System.Threading.Tasks;
using Melville.INPC;
using Melville.Parsing.AwaitConfiguration;
using Melville.Parsing.CountingReaders;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Objects.StreamParts;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.LowLevel.Parsing.ParserContext;
using PdfDirectValue = Melville.Pdf.LowLevel.Model.Objects.PdfDirectValue;

namespace Melville.Pdf.LowLevel.Parsing.ObjectParsers2.IndirectValues;

internal readonly partial struct ObjectStreamParser
{
    [FromConstructor] private readonly ParsingFileOwner owner;
    [FromConstructor] private readonly SubsetParsingReader subsetReader;
    [FromConstructor] private readonly RootObjectParser parser;
    [FromConstructor] private readonly IList<ObjectLocation> locations;
    [FromConstructor] private readonly int desiredObjectNumber;


    public static async ValueTask<ObjectStreamParser> CreateAsync(
        ParsingFileOwner owner, PdfValueStream source, int desiredObjectNumber)
    {
        var sourceStream = await source.StreamContentAsync().CA();
        var reader = new ParsingReader(owner, PipeReader.Create(sourceStream), 0);
        var subsetReader = new SubsetParsingReader(reader);
        var parser = new RootObjectParser(subsetReader);

        var locations = await ObjectStreamOperations.GetIncludedObjectNumbersAsync(
            source, subsetReader).CA();

        return new(owner, subsetReader, parser, locations, desiredObjectNumber);
    }

    public async ValueTask<PdfDirectValue> ParseAsync(PdfDirectValue objectValue)
    {
        for (int i = 0; i < locations.Count; i++)
        {
            var dirValue = await ReadSingleObjectAsync(locations[i], i).CA();
            if (desiredObjectNumber == locations[i].ObjectNumber) objectValue = dirValue;
        }

        return objectValue;
    }

    private async Task<PdfDirectValue> ReadSingleObjectAsync(ObjectLocation location, int i)
    {
        await PositionForNextObjectAsync(location, i).CA();

        var indirValue = await parser.ParseAsync().CA();
        if (!indirValue.TryGetEmbeddedDirectValue(out var dirValue))
            throw new PdfParseException("Object stream member may not be a reference");
        
        owner.NewIndirectResolver.RegisterDirectObject(location.ObjectNumber, 0, dirValue, false);
        
        return dirValue;
    }

    private ValueTask PositionForNextObjectAsync(ObjectLocation location, int i)
    {
        subsetReader.ExclusiveEndPosition = NextOffset(locations, i);
        return subsetReader.AdvanceToLocalPositionAsync(locations[i].Offset);
    }

    private long NextOffset(IList<ObjectLocation> locations, int i) => 
        (i + 1 >= locations.Count) ? long.MaxValue : locations[i + 1].Offset;
}