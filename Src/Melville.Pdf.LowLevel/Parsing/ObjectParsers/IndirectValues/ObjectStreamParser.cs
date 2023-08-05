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

namespace Melville.Pdf.LowLevel.Parsing.ObjectParsers.IndirectValues;

internal partial class ObjectStreamParser: IInternalObjectTarget
{
    [FromConstructor] private readonly ParsingFileOwner owner;
    [FromConstructor] private readonly SubsetParsingReader subsetReader;
    [FromConstructor] private readonly RootObjectParser parser;
    [FromConstructor] private readonly int desiredObjectNumber;
    [FromConstructor] private readonly PdfStream source;
    private PdfDirectObject result = default;

    public static async ValueTask<ObjectStreamParser> CreateAsync(
        ParsingFileOwner owner, PdfStream source, int desiredObjectNumber)
    {
        var sourceStream = await source.StreamContentAsync().CA();
        var reader = new ParsingReader(owner, PipeReader.Create(sourceStream), 0);
        var subsetReader = new SubsetParsingReader(reader);
        var parser = new RootObjectParser(subsetReader);

        
        return new(owner, subsetReader, parser, desiredObjectNumber, source);
    }

    public async ValueTask<PdfDirectObject> ParseAsync(PdfDirectObject priorResult)
    {
        result = priorResult;
        await ObjectStreamOperations.ReportIncludedObjects(source,
            new InternalObjectTargetForStream(this, -1), subsetReader).CA();
        await DeclareObjectStreamObjectAsync(-1, -1, -1, int.MaxValue).CA();
        return result;
    }

    private int priorObjectNumber = -1;
    private int priorOffset = -1;
    public async ValueTask DeclareObjectStreamObjectAsync(
        int objectNumber, int streamObjectNumber, int streamOrdinal, int streamOffset)
    {
        if (priorObjectNumber >= 0)
        {
            await ParseSingleObject(streamOffset).CA();
        }

        priorObjectNumber = objectNumber;
        priorOffset = streamOffset;
    }

    private async ValueTask ParseSingleObject(int nextOffset)
    {
        var obj = await ReadSingleObjectAsync(priorOffset, nextOffset).CA();
        if (priorObjectNumber == desiredObjectNumber) result = obj;
        owner.NewIndirectResolver.RegisterDirectObject(priorObjectNumber, 0, obj, false);
    }

    private async Task<PdfDirectObject> ReadSingleObjectAsync(int objectOffset, int nextOffset)
    {
        await PositionForNextObjectAsync(objectOffset, nextOffset).CA();

        var indirValue = await parser.ParseAsync().CA();
        if (!indirValue.TryGetEmbeddedDirectValue(out var dirValue))
            throw new PdfParseException("Object stream member may not be a reference");
        return dirValue;
    }

    private ValueTask PositionForNextObjectAsync(int startLocation, int nextLocation)
    {
        subsetReader.ExclusiveEndPosition = nextLocation;
        return subsetReader.AdvanceToLocalPositionAsync(startLocation);
    }
}