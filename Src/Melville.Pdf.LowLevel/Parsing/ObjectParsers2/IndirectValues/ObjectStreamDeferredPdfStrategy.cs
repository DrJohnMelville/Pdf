using System;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Threading.Tasks;
using Melville.INPC;
using Melville.Parsing.AwaitConfiguration;
using Melville.Parsing.CountingReaders;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Objects2;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.LowLevel.Parsing.ObjectParsers;
using Melville.Pdf.LowLevel.Parsing.ParserContext;
using Melville.Postscript.Interpreter.Values;

namespace Melville.Pdf.LowLevel.Parsing.ObjectParsers2.IndirectValues;

internal partial class ObjectStreamDeferredPdfStrategy : IIndirectValueSource
{
    [FromConstructor] private readonly ParsingFileOwner owner;

    public string GetValue(in MementoUnion memento) =>
        $"Load from Object Stream # {memento.Int32s[0]} as position {memento.Int32s[1]}";

    public async ValueTask<PdfDirectValue> LookupAsync(MementoUnion memento)
    {
        var source = (await owner.NewIndirectResolver.LookupAsync(memento.Int32s[0], 0).CA()).Get<PdfValueStream>();
        var sourceStream = await source.StreamContentAsync().CA();
        var reader = new ParsingReader(owner, PipeReader.Create(sourceStream), 0);
        var subsetReader = new SubsetParsingReader(reader);
        var parser = new RootObjectParser(subsetReader);

        var locations = await ObjectStreamOperations.GetIncludedObjectNumbersAsync(
            source, subsetReader).CA();

        int desiredOrdinal = memento.Int32s[1];

        PdfDirectValue ret = default;
        for (int i = 0; i < locations.Count; i++)
        {
            var location = locations[i];
            await subsetReader.AdvanceToLocalPositionAsync(location.Offset).CA();
            subsetReader.ExclusiveEndPosition = NextOffset(locations, i);
            var indirValue = await parser.ParseAsync().CA();
            if (!indirValue.TryGetEmbeddedDirectValue(out var dirValue))
                throw new PdfParseException("Object stream member may not be a reference");
            owner.NewIndirectResolver.RegisterDirectObject(location.ObjectNumber, 0, dirValue);
            if (desiredOrdinal == i) ret = dirValue;
        }

        return ret;
    }

    private long NextOffset(IList<ObjectLocation> locations, int i)
    {
        return (i + 1 >= locations.Count) ? long.MaxValue : locations[i + 1].Offset;
    }

    public bool TryGetObjectReference(out int objectNumber, out int generation, MementoUnion memento)
    {
        objectNumber = generation = -1;
        return false;
    }

    public PdfIndirectValue Create(int streamNum, int streamPosition) => 
        new(this, MementoUnion.CreateFrom(streamNum, streamPosition));
}